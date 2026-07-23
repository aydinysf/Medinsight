using MedInsight.Application.Abstractions.Dicom;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.Dicom;

public static class DependencyInjection
{
    public static IServiceCollection AddDicomServices(this IServiceCollection services)
    {
        services.AddSingleton<IDicomMetadataReader, FoDicomMetadataReader>();
        return services;
    }
}
