using MedInsight.Application.Cases;
using MedInsight.Application.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterPatientHandler>();
        services.AddScoped<GetPatientQueryHandler>();
        services.AddScoped<CreateCaseHandler>();

        return services;
    }
}
