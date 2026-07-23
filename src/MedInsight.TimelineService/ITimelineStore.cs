namespace MedInsight.TimelineService;

public interface ITimelineStore
{
    Task AppendAsync(TimelineEntry entry, CancellationToken cancellationToken);

    Task<IReadOnlyList<TimelineEntry>> GetByCaseAsync(Guid caseId, CancellationToken cancellationToken);
}
