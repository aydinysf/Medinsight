using System.Text.Json;
using MedInsight.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MedInsight.Infrastructure.Persistence.Outbox;

public sealed class DomainEventsToOutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            CollectDomainEvents(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            CollectDomainEvents(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void CollectDomainEvents(DbContext context)
    {
        var outboxMessages = context.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DequeueDomainEvents())
            .Select(domainEvent => new OutboxMessage
            {
                EventId = domainEvent.EventId,
                EventClrType = domainEvent.GetType().AssemblyQualifiedName!,
                EventType = domainEvent.EventType,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredAt = domainEvent.OccurredAt,
                CaseId = domainEvent.CaseId,
                CorrelationId = domainEvent.CorrelationId,
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
