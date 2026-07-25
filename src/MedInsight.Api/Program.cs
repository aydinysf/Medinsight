using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using MedInsight.AIOrchestration;
using MedInsight.AIOrchestration.Pipeline;
using MedInsight.Api.Auth;
using MedInsight.Api.Middleware;
using MedInsight.Application;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Matching;
using MedInsight.Application.Quality;
using MedInsight.Dicom;
using MedInsight.Domain.Identity;
using MedInsight.Infrastructure;
using MedInsight.Infrastructure.Persistence;
using MedInsight.TimelineService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Observability (docs/architecture/observability.md): OpenTelemetry + log satırlarında traceId.
builder.Logging.Configure(options =>
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);

var otelBuilder = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("MedInsight.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsql()
            .AddSource("MedInsight.Outbox");

        // OTLP endpoint tanımlıysa dışa aktar (örn. Jaeger/Grafana Tempo); yoksa yalnız log korelasyonu.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["Otel:Endpoint"]))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"]!));
        }
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTimelineService();
builder.Services.AddDicomServices();
builder.Services.AddAiOrchestration();
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.Configure<QualityOptions>(builder.Configuration.GetSection(QualityOptions.SectionName));
builder.Services.Configure<MatchingOptions>(builder.Configuration.GetSection(MatchingOptions.SectionName));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("'Jwt:Key' yapılandırılmamış.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MedInsight";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // SignalR: WebSocket bağlantısında token query string ile gelir.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

// Rate limiting (docs/architecture/rate-limiting-idempotency.md): endpoint bazlı strateji,
// 429 yanıtları Retry-After başlığı taşır (zorunlu).
static string RateLimitPartitionKey(HttpContext context) =>
    context.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? context.Connection.RemoteIpAddress?.ToString()
    ?? "anonymous";

builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    limiter.OnRejected = (context, _) =>
    {
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var value)
            ? ((int)value.TotalSeconds).ToString()
            : "30";
        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        return ValueTask.CompletedTask;
    };

    // Standart IP+kullanıcı limiti (GET dahil tüm istekler)
    limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(RateLimitPartitionKey(context), _ =>
            new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    // Toplu yükleme: kullanıcı başına eşzamanlı ~10 istek sınırı
    limiter.AddPolicy("uploads", context =>
        RateLimitPartition.GetConcurrencyLimiter(RateLimitPartitionKey(context), _ =>
            new ConcurrencyLimiterOptions { PermitLimit = 10, QueueLimit = 10, QueueProcessingOrder = QueueProcessingOrder.OldestFirst }));

    // Mesajlaşma: orta seviye + burst koruması
    limiter.AddPolicy("messages", context =>
        RateLimitPartition.GetSlidingWindowLimiter(RateLimitPartitionKey(context), _ =>
            new SlidingWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 6, QueueLimit = 0 }));

    // Admin onayı: admin başına düşük limit
    limiter.AddPolicy("admin-approve", context =>
        RateLimitPartition.GetFixedWindowLimiter(RateLimitPartitionKey(context), _ =>
            new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

// Frontend SPA için CORS (ADR-017) — yalnız yapılandırılan origin'ler.
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MedInsight API",
        Version = "v1",
        Description = "Clinical Decision Support System (CDSS). MedInsight organizes, compares and "
                    + "analyzes medical records to support physician decision-making. It is not a "
                    + "diagnostic tool and never claims to diagnose disease.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "POST /api/v1/auth/login yanıtındaki accessToken",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("MedInsightDb")!,
        name: "postgresql",
        tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<MedInsightDbContext>().Database.MigrateAsync();
}

// Geliştirme/pilot admin hesabı — yalnızca config'te tanımlıysa ve mevcut değilse oluşturulur.
var adminEmail = app.Configuration["Admin:Email"];
var adminPassword = app.Configuration["Admin:Password"];
if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    if (!await users.EmailExistsAsync(adminEmail))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<MedInsight.Application.Abstractions.Auth.IPasswordHasher>();
        users.Add(User.Create("Sistem Yöneticisi", adminEmail, UserRole.Admin, hasher.Hash(adminPassword)));
        await scope.ServiceProvider.GetRequiredService<MedInsightDbContext>().SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MedInsight API v1");
    });
}

app.UseHttpsRedirection();
if (corsOrigins.Length > 0)
{
    app.UseCors("frontend");
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<MedInsight.Api.Hubs.ConsultationHub>("/hubs/consultations");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();
