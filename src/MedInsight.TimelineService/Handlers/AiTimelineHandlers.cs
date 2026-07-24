using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.TimelineService.Handlers;

public sealed class OnAIAnalysisCompleted(ITimelineStore store) : IDomainEventHandler<AIAnalysisCompleted>
{
    public Task HandleAsync(AIAnalysisCompleted e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"AI ön analizi tamamlandı ({e.FindingIds.Count} bulgu)", e.EventId),
            ct);
}

public sealed class OnDoctorReviewPriorityRaised(ITimelineStore store) : IDomainEventHandler<DoctorReviewPriorityRaised>
{
    public Task HandleAsync(DoctorReviewPriorityRaised e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(e.CaseId!.Value, e.EventType, e.OccurredAt, $"Doktor inceleme önceliği yükseltildi: {e.Reason}", e.EventId),
            ct);
}

public sealed class OnHealthRouteSnapshotCreated(ITimelineStore store) : IDomainEventHandler<HealthRouteSnapshotCreated>
{
    public Task HandleAsync(HealthRouteSnapshotCreated e, CancellationToken ct) =>
        store.AppendAsync(
            TimelineEntry.Create(
                e.CaseId!.Value,
                e.EventType,
                e.OccurredAt,
                $"Sağlık rotası güncellendi (v{e.VersionNumber}, {e.TriggeredBy}): {e.Status} → sıradaki adım: {e.NextStep}",
                e.EventId),
            ct);
}
