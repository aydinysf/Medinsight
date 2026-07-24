using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Application.HealthRoutes;

/// <summary>
/// Health Route Engine abonesi: AI analizi tamamlandığında rotayı yeni snapshot'la
/// günceller (ADR-002). Snapshot'ın arkasındaki kararın doğruluğu bu motorun
/// sorumluluğu değildir (bounded-contexts-overview.md).
/// </summary>
public sealed class OnAIAnalysisCompletedUpdateRoute(ICaseRepository cases) : IDomainEventHandler<AIAnalysisCompleted>
{
    public async Task HandleAsync(AIAnalysisCompleted e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        if (medicalCase is null)
        {
            return;
        }

        // idempotency: bu analiz için snapshot zaten üretilmişse tekrar üretme
        if (medicalCase.HealthRouteSnapshots.Any(s => s.TriggerSourceId == e.AnalysisId))
        {
            return;
        }

        var analysis = medicalCase.AiAnalyses.FirstOrDefault(a => a.Id == e.AnalysisId);
        var riskLevel = analysis?.DifferentialDiagnoses.Count > 0
            ? analysis.DifferentialDiagnoses.Max(d => d.RiskLevel)
            : medicalCase.RiskLevel;

        medicalCase.UpdateHealthRoute(
            status: "AI ön analizi tamamlandı",
            nextStep: "Doktor incelemesi",
            riskLevel: riskLevel,
            triggeredBy: RouteTrigger.AI,
            triggerSourceId: e.AnalysisId,
            reason: $"AI analizi tamamlandı (model: {e.ModelVersion})");

        await cases.SaveChangesAsync(cancellationToken);
    }
}
