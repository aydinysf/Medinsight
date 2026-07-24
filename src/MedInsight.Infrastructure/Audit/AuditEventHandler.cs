using System.Text.Json;
using MedInsight.Domain.Common;
using MedInsight.Infrastructure.Persistence;

namespace MedInsight.Infrastructure.Audit;

/// <summary>
/// Açık-jenerik abone: HER domain event bir audit kaydı üretir. Böylece
/// "audit loglamayı unutmak" yapısal olarak imkansızdır (audit-service.md, Kim Yazar).
/// </summary>
public sealed class AuditEventHandler<TEvent>(MedInsightDbContext db) : IDomainEventHandler<TEvent>
    where TEvent : DomainEvent
{
    private static readonly string[] ActorPropertyNames =
        ["UploadedByUserId", "SenderUserId", "VerifiedByAdminId", "CreatedByDoctorId", "DoctorId", "UserId"];

    public async Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken)
    {
        // Idempotency: aynı event için ikinci kayıt atılmaz (at-least-once teslim).
        var exists = db.Set<AuditLog>().Any(a => a.Id == domainEvent.EventId);
        if (exists)
        {
            return;
        }

        var payload = JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType());

        db.Set<AuditLog>().Add(new AuditLog
        {
            Id = domainEvent.EventId, // event başına tek kayıt — PK ile garanti
            ActorId = ExtractActor(payload),
            Action = domainEvent.EventType,
            EntityType = domainEvent.CaseId is null ? null : "Case",
            EntityId = domainEvent.CaseId,
            OccurredAtUtc = domainEvent.OccurredAt,
            MetadataJson = payload.GetRawText(),
            CorrelationId = domainEvent.CorrelationId,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Guid? ExtractActor(JsonElement payload)
    {
        foreach (var name in ActorPropertyNames)
        {
            if (payload.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && value.TryGetGuid(out var id))
            {
                return id;
            }
        }

        return null;
    }
}
