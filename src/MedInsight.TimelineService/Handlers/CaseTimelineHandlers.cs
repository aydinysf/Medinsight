using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.TimelineService.Handlers;

public sealed class OnCaseCreated(ITimelineStore store) : IDomainEventHandler<CaseCreated>
{
    public Task HandleAsync(CaseCreated e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Vaka oluşturuldu: {e.Title}", e.EventId),
            ct);
}

public sealed class OnCaseStatusChanged(ITimelineStore store) : IDomainEventHandler<CaseStatusChanged>
{
    public Task HandleAsync(CaseStatusChanged e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(
                e.CaseId!.Value,
                e.EventType,
                e.OccurredAt,
                $"Vaka durumu değişti: {e.FromStatus} → {e.ToStatus}" + (e.Reason is null ? string.Empty : $" ({e.Reason})"),
                e.EventId),
            ct);
}

public sealed class OnCaseReopened(ITimelineStore store) : IDomainEventHandler<CaseReopened>
{
    public Task HandleAsync(CaseReopened e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Vaka yeniden açıldı: {e.Reason}", e.EventId),
            ct);
}

public sealed class OnDocumentUploaded(ITimelineStore store) : IDomainEventHandler<DocumentUploaded>
{
    public Task HandleAsync(DocumentUploaded e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, "Vakaya belge yüklendi", e.EventId, e.UploadedByUserId),
            ct);
}
