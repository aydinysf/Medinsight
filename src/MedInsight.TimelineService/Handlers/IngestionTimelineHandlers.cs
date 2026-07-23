using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.TimelineService.Handlers;

public sealed class OnDocumentClassified(ITimelineStore store) : IDomainEventHandler<DocumentClassified>
{
    public Task HandleAsync(DocumentClassified e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Belge sınıflandırıldı: {e.DocumentType}", e.EventId),
            ct);
}

public sealed class OnDocumentClassificationFailed(ITimelineStore store) : IDomainEventHandler<DocumentClassificationFailed>
{
    public Task HandleAsync(DocumentClassificationFailed e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Belge sınıflandırılamadı: {e.Reason}", e.EventId),
            ct);
}

public sealed class OnDocumentQualityScored(ITimelineStore store) : IDomainEventHandler<DocumentQualityScored>
{
    public Task HandleAsync(DocumentQualityScored e, CancellationToken ct)
    {
        var summary = e.IsSufficient
            ? $"Belge kalite kontrolünden geçti (skor: {e.OverallScore:0.00})"
            : $"Belge kalite kontrolünden geçemedi (skor: {e.OverallScore:0.00}) — {string.Join("; ", e.FailureReasons)}";

        return store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, summary, e.EventId),
            ct);
    }
}
