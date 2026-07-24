using System.Text;
using System.Text.Json.Serialization;
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

var builder = WebApplication.CreateBuilder(args);

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
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtIssuer,
        ClockSkew = TimeSpan.FromSeconds(30),
    });
builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();
