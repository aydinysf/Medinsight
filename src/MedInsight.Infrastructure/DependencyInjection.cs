using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Abstractions.TextExtraction;
using MedInsight.Infrastructure.Auth;
using MedInsight.Infrastructure.Ingestion;
using MedInsight.Infrastructure.Persistence;
using MedInsight.Infrastructure.TextExtraction;
using MedInsight.Infrastructure.Persistence.Outbox;
using MedInsight.Infrastructure.Repositories;
using MedInsight.Infrastructure.Storage;
using MedInsight.Infrastructure.Timeline;
using MedInsight.TimelineService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MedInsightDb")
            ?? throw new InvalidOperationException("Connection string 'MedInsightDb' is not configured.");

        services.AddSingleton<DomainEventsToOutboxInterceptor>();

        services.AddDbContext<MedInsightDbContext>((provider, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(MedInsightDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(provider.GetRequiredService<DomainEventsToOutboxInterceptor>()));

        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IObjectStorage, MinioObjectStorage>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<ITimelineStore, EfTimelineStore>();

        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        if (string.Equals(configuration["Ocr:Provider"], "Tesseract", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IOcrProvider, TesseractOcrProvider>();
        }
        else
        {
            services.AddSingleton<IOcrProvider, StubOcrProvider>();
        }

        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<DicomGroupingWindowProcessor>();

        // IAiService implementation (Python AI service client) will be registered here.
        return services;
    }
}
