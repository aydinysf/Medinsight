using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.TimelineService.Handlers;

public sealed class OnDicomStudyGrouped(ITimelineStore store) : IDomainEventHandler<DicomStudyGrouped>
{
    public Task HandleAsync(DicomStudyGrouped e, CancellationToken ct)
    {
        var totalSlices = e.SeriesList.Sum(s => s.SliceCount);
        return store.AppendAsync(
            TimelineEntry.Create(
                e.CaseId!.Value,
                e.EventType,
                e.OccurredAt,
                $"DICOM çalışması gruplandı: {e.SeriesList.Count} seri, {totalSlices} kesit",
                e.EventId),
            ct);
    }
}

public sealed class OnRoutingDecided(ITimelineStore store) : IDomainEventHandler<RoutingDecided>
{
    public Task HandleAsync(RoutingDecided e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Belge işleme yoluna yönlendirildi: {e.Route}", e.EventId),
            ct);
}
