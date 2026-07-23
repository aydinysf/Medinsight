using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases.Events;

public sealed record DocumentClassified : DomainEvent
{
    public required Guid DocumentId { get; init; }

    public required DocumentType DocumentType { get; init; }
}

public sealed record DocumentClassificationFailed : DomainEvent
{
    public required Guid DocumentId { get; init; }

    public required string Reason { get; init; }
}

public sealed record DocumentQualityScored : DomainEvent
{
    public required Guid DocumentId { get; init; }

    public required decimal OverallScore { get; init; }

    public required Dictionary<string, decimal> CriteriaScores { get; init; }

    public required List<string> FailureReasons { get; init; }

    public required bool IsSufficient { get; init; }
}
