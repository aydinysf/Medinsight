namespace MedInsight.Application.Abstractions.Storage;

/// <summary>
/// S3-uyumlu object storage soyutlaması (bkz. docs/backend/tech-stack.md).
/// Geliştirmede MinIO; sağlayıcı değişimi konfigürasyondur, kod değişikliği değil.
/// </summary>
public interface IObjectStorage
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<byte[]> DownloadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>İç servislerin (örn. radyoloji) nesneyi doğrudan çekebilmesi için süreli okuma URL'si.</summary>
    Task<string> GetPresignedReadUrlAsync(string key, TimeSpan validFor, CancellationToken cancellationToken = default);
}
