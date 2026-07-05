using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Infrastructure.Persistence;
using MedInsight.Infrastructure.Repositories;
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

        services.AddDbContext<MedInsightDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(MedInsightDbContext).Assembly.FullName)));

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IMedicalCaseRepository, MedicalCaseRepository>();

        // IAiService implementation (Python AI service client) will be registered here.
        return services;
    }
}
