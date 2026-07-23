using MedInsight.Application.Abstractions.Storage;
using MedInsight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Infrastructure.Storage;

public sealed class IdempotencyRecord
{
    public string Key { get; init; } = null!;

    public string ResponseJson { get; init; } = null!;

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}

public sealed class EfIdempotencyStore(MedInsightDbContext db) : IIdempotencyStore
{
    public async Task<string?> TryGetResponseAsync(string key, CancellationToken cancellationToken = default)
    {
        var record = await db.Set<IdempotencyRecord>().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
        return record?.ResponseJson;
    }

    public async Task SaveResponseAsync(string key, string responseJson, CancellationToken cancellationToken = default)
    {
        db.Set<IdempotencyRecord>().Add(new IdempotencyRecord { Key = key, ResponseJson = responseJson });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Yarış durumu: aynı anahtar eşzamanlı yazıldı — ilk kayıt kazanır, sorun değil.
        }
    }
}
