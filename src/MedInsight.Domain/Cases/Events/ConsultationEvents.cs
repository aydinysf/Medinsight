using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases.Events;

public sealed record ConsultationStarted : DomainEvent
{
    public required Guid ConsultationId { get; init; }

    public required Guid DoctorId { get; init; }
}

public sealed record ConsultationCompleted : DomainEvent
{
    public required Guid ConsultationId { get; init; }

    public required Guid DoctorId { get; init; }
}

/// <summary>İçerik taşımaz — gizlilik (bkz. domain-events-catalog.md).</summary>
public sealed record ConsultationMessageSent : DomainEvent
{
    public required Guid MessageId { get; init; }

    public required Guid ConsultationId { get; init; }

    public required Guid SenderUserId { get; init; }
}

public sealed record ClinicalNoteAdded : DomainEvent
{
    public required Guid NoteId { get; init; }

    public required Guid ConsultationId { get; init; }

    public required Guid DoctorId { get; init; }
}

public sealed record AIAnalysisReviewed : DomainEvent
{
    public required Guid AnalysisId { get; init; }

    public required Guid DoctorId { get; init; }

    public required AnalysisReviewDecision Decision { get; init; }

    public string? CorrectionNotes { get; init; }
}

public sealed record TreatmentPlanCreated : DomainEvent
{
    public required Guid TreatmentId { get; init; }

    public required Guid ConsultationId { get; init; }

    public required Guid CreatedByDoctorId { get; init; }
}

public enum EscalationReason
{
    HighRiskWithUnvalidatedFinding = 0,
    DoctorRequested = 1,
}

/// <summary>MVP'de vendor API çağrısı tetiklemez — önceliklendirme + not (ADR-014).</summary>
public sealed record EscalationSuggested : DomainEvent
{
    public required EscalationReason Reason { get; init; }
}

public enum AnalysisReviewDecision
{
    Approved = 0,
    Corrected = 1,
}
