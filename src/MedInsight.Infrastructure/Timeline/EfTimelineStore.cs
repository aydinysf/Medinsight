using MedInsight.Infrastructure.Persistence;
using MedInsight.TimelineService;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Timeline;

public sealed class EfTimelineStore(MedInsightDbContext db) : ITimelineStore
{
    public async Task AppendAsync(TimelineEntry entry, CancellationToken cancellationToken)
    {
        // Idempotency: aynı source event iki kez işlenirse ikinci kayıt atlanır (at-least-once teslim).
        var exists = await db.TimelineEntries.AsNoTracking()
            .AnyAsync(e => e.SourceEventId == entry.SourceEventId, cancellationToken);
        if (exists)
        {
            return;
        }

        db.TimelineEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimelineEntry>> GetByCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        await db.TimelineEntries.AsNoTracking()
            .Where(e => e.CaseId == caseId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
}
