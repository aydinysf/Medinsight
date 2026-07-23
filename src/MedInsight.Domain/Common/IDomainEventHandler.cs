namespace MedInsight.Domain.Common;

/// <summary>
/// Domain event abonelerinin uyguladığı saf arayüz. Domain katmanı hiçbir
/// pakete bağımlı olmadığı için dispatch mekanizması Infrastructure'dadır.
/// </summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
