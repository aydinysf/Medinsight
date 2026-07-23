namespace MedInsight.Infrastructure.Persistence.Outbox;

/// <summary>
/// Domain event'ler aggregate kaydıyla aynı transaction'da bu tabloya yazılır;
/// OutboxProcessor arka planda abonelere dağıtır (at-least-once).
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid EventId { get; init; }

    /// <summary>Event'in assembly-qualified CLR tipi — deserializasyon için.</summary>
    public string EventClrType { get; init; } = null!;

    public string EventType { get; init; } = null!;

    public string Payload { get; init; } = null!;

    public DateTime OccurredAt { get; init; }

    public Guid? CaseId { get; init; }

    public Guid CorrelationId { get; init; }

    public DateTime? ProcessedAtUtc { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }
}
