using MedInsight.Application.Admin;
using MedInsight.Application.Analyses;
using MedInsight.Application.Auth;
using MedInsight.Application.Cases;
using MedInsight.Application.Consultations;
using MedInsight.Application.Doctors;
using MedInsight.Application.Documents;
using MedInsight.Application.HealthRoutes;
using MedInsight.Application.Ingestion;
using MedInsight.Application.Matching;
using MedInsight.Application.Notifications;
using MedInsight.Application.Patients;
using MedInsight.Application.Quality;
using MedInsight.Application.Radiology;
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
        services.AddScoped<GetMyDoctorProfileQueryHandler>();
        services.AddScoped<GetDoctorReviewQueueQueryHandler>();
        services.AddScoped<CloseCaseHandler>();
        services.AddScoped<ReopenCaseHandler>();
        services.AddScoped<ListPendingVerificationsQueryHandler>();
        services.AddScoped<ApproveVerificationHandler>();
        services.AddScoped<RejectVerificationHandler>();
        services.AddSingleton<DoctorMatchingEngine>();
        services.AddScoped<GetDoctorMatchesQueryHandler>();
        services.AddScoped<GetImageFindingsQueryHandler>();
        services.AddScoped<DoctorActionContext>();
        services.AddScoped<StartConsultationHandler>();
        services.AddScoped<SendConsultationMessageHandler>();
        services.AddScoped<GetConsultationMessagesQueryHandler>();
        services.AddScoped<GetCaseConsultationsQueryHandler>();
        services.AddScoped<AddClinicalNoteHandler>();
        services.AddScoped<CompleteConsultationHandler>();
        services.AddScoped<ReviewAiAnalysisHandler>();
        services.AddScoped<CreateTreatmentPlanHandler>();
        services.AddScoped<RequestEscalationHandler>();

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

        // Radiology Inference aboneleri (ADR-010, ADR-014)
        services.AddScoped<IDomainEventHandler<DicomStudyGrouped>, OnDicomStudyGroupedRunInference>();
        services.AddScoped<IDomainEventHandler<ImageFindingAdded>, OnImageFindingAddedEscalationCheck>();

        // Identity & Verification abonesi (reviewer-profile.md)
        services.AddScoped<IDomainEventHandler<MedInsight.Domain.Identity.Events.DoctorVerified>, OnDoctorVerifiedCreateReviewerProfile>();

        // Konsültasyon aboneleri: müsaitlik sayacı (ADR-009), ReviewerProfile, escalation (ADR-014)
        services.AddScoped<IDomainEventHandler<ConsultationStarted>, OnConsultationStartedUpdateAvailability>();
        services.AddScoped<IDomainEventHandler<ConsultationCompleted>, OnConsultationCompletedUpdateAvailability>();
        services.AddScoped<IDomainEventHandler<AIAnalysisReviewed>, OnAIAnalysisReviewedUpdateReviewerProfile>();
        services.AddScoped<IDomainEventHandler<AIAnalysisCompleted>, OnAIAnalysisCompletedEscalationCheck>();

        // Notification Engine aboneleri (notification-engine.md)
        services.AddScoped<IDomainEventHandler<DocumentClassificationFailed>, NotifyOnDocumentClassificationFailed>();
        services.AddScoped<IDomainEventHandler<AIAnalysisCompleted>, NotifyOnAIAnalysisCompleted>();
        services.AddScoped<IDomainEventHandler<DoctorReviewPriorityRaised>, NotifyOnDoctorReviewPriorityRaised>();
        services.AddScoped<IDomainEventHandler<ConsultationMessageSent>, NotifyOnConsultationMessageSent>();
        services.AddScoped<IDomainEventHandler<MedInsight.Domain.Identity.Events.DoctorVerified>, NotifyOnDoctorVerified>();
        services.AddScoped<IDomainEventHandler<MedInsight.Domain.Identity.Events.DoctorVerificationRejected>, NotifyOnDoctorVerificationRejected>();

        return services;
    }
}
