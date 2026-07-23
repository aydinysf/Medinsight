using MedInsight.Application.Auth;
using MedInsight.Application.Cases;
using MedInsight.Application.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginHandler>();
        services.AddScoped<RegisterPatientHandler>();
        services.AddScoped<GetPatientQueryHandler>();
        services.AddScoped<CreateCaseHandler>();
        services.AddScoped<GetCaseQueryHandler>();
        services.AddScoped<GetPatientCasesQueryHandler>();

        return services;
    }
}
