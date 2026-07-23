namespace MedInsight.TimelineService;

/// <summary>
/// Append-only timeline kaydı (ADR-006, docs/architecture/timeline-service.md).
/// Hiçbir kayıt güncellenmez veya silinmez; sadece eklenir.
/// </summary>
public sealed class TimelineEntry
{
    private TimelineEntry()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid CaseId { get; private set; }

    public string EventType { get; private set; } = null!;

    public DateTime OccurredAt { get; private set; }

    public string Summary { get; private set; } = null!;

    public Guid SourceEventId { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public static TimelineEntry Create(Guid caseId, string eventType, DateTime occurredAt, string summary, Guid sourceEventId, Guid? actorUserId = null)
    {
        return new TimelineEntry
        {
            CaseId = caseId,
            EventType = eventType,
            OccurredAt = occurredAt,
            Summary = summary,
            SourceEventId = sourceEventId,
            ActorUserId = actorUserId,
        };
    }
}
