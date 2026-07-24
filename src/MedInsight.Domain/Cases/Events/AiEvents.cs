using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases.Events;

public sealed record AIAnalysisRequested : DomainEvent
{
    public required List<Guid> DocumentIds { get; init; }
}

public sealed record AIAnalysisCompleted : DomainEvent
{
    public required Guid AnalysisId { get; init; }

    public required string ModelVersion { get; init; }

    public required string PromptVersion { get; init; }

    public required decimal ConfidenceScore { get; init; }

    public required List<Guid> FindingIds { get; init; }
}

/// <summary>ADR-004: düşük confidence dalı — Notification tarafından bağımsız işlenir.</summary>
public sealed record DoctorReviewPriorityRaised : DomainEvent
{
    public required Guid AnalysisId { get; init; }

    public required string Reason { get; init; }
}

public sealed record HealthRouteSnapshotCreated : DomainEvent
{
    public required Guid SnapshotId { get; init; }

    public Guid? PreviousVersionId { get; init; }

    public required int VersionNumber { get; init; }

    public required string Status { get; init; }

    public required string NextStep { get; init; }

    public required RiskLevel RiskLevel { get; init; }

    public required RouteTrigger TriggeredBy { get; init; }

    public Guid? TriggerSourceId { get; init; }

    public required string Reason { get; init; }
}
