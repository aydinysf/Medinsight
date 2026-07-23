using System.Text.Json;
using MedInsight.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedInsight.Infrastructure.Persistence.Outbox;

/// <summary>
/// Outbox'taki işlenmemiş event'leri IDomainEventHandler abonelerine dağıtır.
/// At-least-once teslim; handler'lar idempotent olmalıdır.
/// </summary>
public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox islemede beklenmeyen hata");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedInsightDbContext>();

        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAtUtc == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await DispatchAsync(scope.ServiceProvider, message, cancellationToken);
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogWarning(ex, "Outbox event dagitilamadi: {EventType} ({EventId}), deneme {Retry}", message.EventType, message.EventId, message.RetryCount);
            }
        }

        if (messages.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task DispatchAsync(IServiceProvider services, OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = Type.GetType(message.EventClrType)
            ?? throw new InvalidOperationException($"Event tipi çözülemedi: {message.EventClrType}");

        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException($"Event payload'ı deserialize edilemedi: {message.EventId}");

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<DomainEvent>.HandleAsync))!;

        foreach (var handler in services.GetServices(handlerType))
        {
            if (handler is null)
            {
                continue;
            }

            await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
        }
    }
}
