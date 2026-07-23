namespace MedInsight.Application.Abstractions.Storage;

/// <summary>
/// Idempotency-Key desteği (bkz. docs/architecture/rate-limiting-idempotency.md):
/// aynı anahtarla tekrarlanan istek yeniden işlenmez, ilk sonuç döner.
/// </summary>
public interface IIdempotencyStore
{
    Task<string?> TryGetResponseAsync(string key, CancellationToken cancellationToken = default);

    Task SaveResponseAsync(string key, string responseJson, CancellationToken cancellationToken = default);
}
