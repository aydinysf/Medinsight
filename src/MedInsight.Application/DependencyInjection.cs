using MedInsight.Application.MedicalCases;
using MedInsight.Application.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreatePatientService>();
        services.AddScoped<CreateMedicalCaseService>();

        return services;
    }
}
