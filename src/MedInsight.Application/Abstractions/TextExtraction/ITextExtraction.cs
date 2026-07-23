namespace MedInsight.Application.Abstractions.TextExtraction;

public sealed record OcrResult(string Text, decimal ConfidenceScore, string Provider);

/// <summary>
/// OCR sağlayıcı soyutlaması (ADR-011): karar ertelendi, sağlayıcı değişimi
/// konfigürasyondur. MVP varsayılanı Tesseract; kalıcı config anahtarı, feature flag değil.
/// </summary>
public interface IOcrProvider
{
    string Name { get; }

    Task<OcrResult> ExtractTextAsync(byte[] content, CancellationToken cancellationToken = default);
}

/// <summary>Metin katmanlı PDF'ten doğrudan metin çıkarma — OCR gerekmez (ingestion-pipeline.md, Routing).</summary>
public interface IPdfTextExtractor
{
    /// <summary>Metin çıkarılamazsa null döner.</summary>
    string? ExtractText(byte[] pdfContent);
}
