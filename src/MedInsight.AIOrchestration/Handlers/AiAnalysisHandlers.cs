using MedInsight.AIOrchestration.Pipeline;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;
using Microsoft.Extensions.Options;

namespace MedInsight.AIOrchestration.Handlers;

/// <summary>AI Analysis Engine girişi: AIAnalysisRequested → Hızır orkestrasyonu → AddAiAnalysis.</summary>
public sealed class OnAIAnalysisRequested(ICaseRepository cases, HizirOrchestrator orchestrator) : IDomainEventHandler<AIAnalysisRequested>
{
    public async Task HandleAsync(AIAnalysisRequested e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null || medicalCase.Status != CaseStatus.AIAnalysis)
        {
            return; // idempotency: analiz zaten yapılmış (durum DoctorReview'e geçmiş) veya vaka yok
        }

        var result = await orchestrator.AnalyzeAsync(medicalCase, cancellationToken);

        medicalCase.AddAiAnalysis(
            result.ModelVersion,
            result.PromptVersion,
            result.ConfidenceScore,
            result.Summary,
            result.PatientMessage,
            result.Findings,
            result.DifferentialDiagnoses);

        await cases.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// ADR-004: confidence eşik kontrolü. Eşik altındaysa doktor inceleme önceliği
/// yükselir; hasta bildirimi Notification Engine'in bağımsız aboneliğidir.
/// </summary>
public sealed class OnAIAnalysisCompletedConfidenceCheck(ICaseRepository cases, IOptions<AiOptions> options) : IDomainEventHandler<AIAnalysisCompleted>
{
    public async Task HandleAsync(AIAnalysisCompleted e, CancellationToken cancellationToken)
    {
        if (e.ConfidenceScore >= options.Value.ConfidenceThreshold)
        {
            return;
        }

        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null || medicalCase.ReviewPriority == ReviewPriority.High)
        {
            return; // idempotency
        }

        medicalCase.EscalateReviewPriority(e.AnalysisId, $"Düşük güven skoru ({e.ConfidenceScore:0.00})");
        await cases.SaveChangesAsync(cancellationToken);
    }
}
