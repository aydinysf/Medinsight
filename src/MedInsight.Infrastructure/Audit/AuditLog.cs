namespace MedInsight.Infrastructure.Audit;

/// <summary>
/// Değiştirilemez denetim kaydı (bkz. docs/architecture/audit-service.md).
/// UPDATE/DELETE veritabanı seviyesinde trigger ile engellidir — konvansiyon değil.
/// KVKK: "kim, ne zaman, neyi değiştirdi" sorusunun tek kaynağı.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Aksiyonu yapan kullanıcı/aktör (event payload'ından çözülür; sistem aksiyonlarında null).</summary>
    public Guid? ActorId { get; init; }

    public string Action { get; init; } = null!;

    public string? EntityType { get; init; }

    public Guid? EntityId { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public string? IpAddress { get; init; }

    public string MetadataJson { get; init; } = null!;

    public Guid CorrelationId { get; init; }
}
