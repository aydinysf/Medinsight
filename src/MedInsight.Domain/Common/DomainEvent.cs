namespace MedInsight.Domain.Common;

/// <summary>
/// Ortak event zarfı (bkz. docs/domain/domain-events-catalog.md — Genel Kural):
/// eventId, eventType, occurredAt, caseId, causationId, correlationId.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public string EventType => GetType().Name;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid? CaseId { get; init; }

    public Guid? CausationId { get; init; }

    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
