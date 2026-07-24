using MedInsight.Application.Admin;
using MedInsight.Application.Analyses;
using MedInsight.Application.Auth;
using MedInsight.Application.Cases;
using MedInsight.Application.Doctors;
using MedInsight.Application.Documents;
using MedInsight.Application.HealthRoutes;
using MedInsight.Application.Ingestion;
using MedInsight.Application.Matching;
using MedInsight.Application.Patients;
using MedInsight.Application.Quality;
using MedInsight.Application.Quality.Criteria;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
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
        services.AddScoped<UploadDocumentsHandler>();
        services.AddScoped<GetCaseDocumentsQueryHandler>();
        services.AddScoped<GetHealthRouteQueryHandler>();
        services.AddScoped<GetHealthRouteSnapshotsQueryHandler>();
        services.AddScoped<GetCaseAnalysesQueryHandler>();
        services.AddScoped<RegisterDoctorHandler>();
        services.AddScoped<SubmitVerificationHandler>();
        services.AddScoped<SetAvailabilityHandler>();
        services.AddScoped<ListPendingVerificationsQueryHandler>();
        services.AddScoped<ApproveVerificationHandler>();
        services.AddScoped<RejectVerificationHandler>();
        services.AddSingleton<DoctorMatchingEngine>();
        services.AddScoped<GetDoctorMatchesQueryHandler>();

        // Document Quality Engine — her kriter bağımsız plugin (document-quality-engine.md)
        services.AddSingleton<IQualityCriterion, DuplicatedFilesCriterion>();
        services.AddSingleton<IQualityCriterion, CompletenessCriterion>();
        services.AddSingleton<IQualityCriterion, DicomIntegrityCriterion>();
        services.AddSingleton<IQualityCriterion, OcrScoreCriterion>();
        services.AddScoped<QualityEngine>();

        // Ingestion pipeline event aboneleri
        services.AddScoped<IDomainEventHandler<DocumentUploaded>, OnDocumentUploadedClassify>();
        services.AddScoped<IDomainEventHandler<DocumentClassified>, OnDocumentClassifiedRunQuality>();
        services.AddScoped<IDomainEventHandler<DocumentClassified>, OnDocumentClassifiedGroupDicom>();
        services.AddScoped<IDomainEventHandler<DocumentQualityScored>, OnDocumentQualityScoredRoute>();
        services.AddScoped<IDomainEventHandler<RoutingDecided>, OnRoutingDecidedExtractText>();

        // Health Route Engine abonesi (ADR-002)
        services.AddScoped<IDomainEventHandler<AIAnalysisCompleted>, OnAIAnalysisCompletedUpdateRoute>();

        // Identity & Verification abonesi (reviewer-profile.md)
        services.AddScoped<IDomainEventHandler<MedInsight.Domain.Identity.Events.DoctorVerified>, OnDoctorVerifiedCreateReviewerProfile>();

        return services;
    }
}
