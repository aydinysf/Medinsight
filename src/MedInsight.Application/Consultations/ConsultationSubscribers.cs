using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Application.Consultations;

/// <summary>ADR-009: ActiveCaseCount, ConsultationStarted/Completed event'leriyle güncellenir.</summary>
public sealed class OnConsultationStartedUpdateAvailability(IDoctorRepository doctors) : IDomainEventHandler<ConsultationStarted>
{
    public async Task HandleAsync(ConsultationStarted e, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(e.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        doctor.IncrementActiveCases();
        await doctors.SaveChangesAsync(cancellationToken);
    }
}

public sealed class OnConsultationCompletedUpdateAvailability(IDoctorRepository doctors) : IDomainEventHandler<ConsultationCompleted>
{
    public async Task HandleAsync(ConsultationCompleted e, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(e.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        doctor.DecrementActiveCases();
        await doctors.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>AIAnalysisReviewed → ReviewerProfile (Learning Loop girdisi, reviewer-profile.md).</summary>
public sealed class OnAIAnalysisReviewedUpdateReviewerProfile(IDoctorRepository doctors) : IDomainEventHandler<AIAnalysisReviewed>
{
    public async Task HandleAsync(AIAnalysisReviewed e, CancellationToken cancellationToken)
    {
        var profile = await doctors.GetReviewerProfileForUpdateAsync(e.DoctorId, cancellationToken);
        if (profile is null)
        {
            return;
        }

        profile.RecordReview(corrected: e.Decision == AnalysisReviewDecision.Corrected);
        await doctors.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// ADR-014 otomatik tetikleme: High/Critical tanı adayı + OpenSourceImageModel
/// bulgusu birlikteyse EscalationSuggested. Sistem önerir, karar vermez.
/// </summary>
public sealed class OnAIAnalysisCompletedEscalationCheck(ICaseRepository cases) : IDomainEventHandler<AIAnalysisCompleted>
{
    public async Task HandleAsync(AIAnalysisCompleted e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var analysis = medicalCase?.AiAnalyses.FirstOrDefault(a => a.Id == e.AnalysisId);
        if (medicalCase is null || analysis is null)
        {
            return;
        }

        var hasHighRiskDifferential = analysis.DifferentialDiagnoses.Any(d => d.RiskLevel >= RiskLevel.High);
        var hasUnvalidatedFinding = analysis.Findings.Any(f => f.Source == AiFindingSource.OpenSourceImageModel);
        if (!hasHighRiskDifferential || !hasUnvalidatedFinding)
        {
            return;
        }

        medicalCase.SuggestEscalation(EscalationReason.HighRiskWithUnvalidatedFinding);
        await cases.SaveChangesAsync(cancellationToken);
    }
}
