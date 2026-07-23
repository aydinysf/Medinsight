using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
using MedInsight.TimelineService.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace MedInsight.TimelineService;

public static class DependencyInjection
{
    public static IServiceCollection AddTimelineService(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<CaseCreated>, OnCaseCreated>();
        services.AddScoped<IDomainEventHandler<CaseStatusChanged>, OnCaseStatusChanged>();
        services.AddScoped<IDomainEventHandler<CaseReopened>, OnCaseReopened>();
        services.AddScoped<IDomainEventHandler<DocumentUploaded>, OnDocumentUploaded>();
        services.AddScoped<IDomainEventHandler<DocumentClassified>, OnDocumentClassified>();
        services.AddScoped<IDomainEventHandler<DocumentClassificationFailed>, OnDocumentClassificationFailed>();
        services.AddScoped<IDomainEventHandler<DocumentQualityScored>, OnDocumentQualityScored>();
        services.AddScoped<IDomainEventHandler<DicomStudyGrouped>, OnDicomStudyGrouped>();
        services.AddScoped<IDomainEventHandler<RoutingDecided>, OnRoutingDecided>();

        return services;
    }
}
