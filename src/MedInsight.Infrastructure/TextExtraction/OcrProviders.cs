using MedInsight.Application.Abstractions.TextExtraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace MedInsight.Infrastructure.TextExtraction;

/// <summary>
/// Açık kaynak OCR — MVP varsayılanı (ADR-011). `Ocr:TessDataPath` altında
/// dil dosyaları (tur/eng .traineddata) gerektirir.
/// </summary>
public sealed class TesseractOcrProvider(IConfiguration configuration, ILogger<TesseractOcrProvider> logger) : IOcrProvider
{
    public string Name => "Tesseract";

    public Task<OcrResult> ExtractTextAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var tessDataPath = configuration["Ocr:TessDataPath"]
            ?? throw new InvalidOperationException("'Ocr:TessDataPath' yapılandırılmamış.");
        var language = configuration["Ocr:Language"] ?? "tur+eng";

        try
        {
            using var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
            using var image = Pix.LoadFromMemory(content);
            using var page = engine.Process(image);

            var text = page.GetText()?.Trim() ?? string.Empty;
            var confidence = Math.Round((decimal)page.GetMeanConfidence(), 4);
            return Task.FromResult(new OcrResult(text, confidence, Name));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Tesseract OCR başarısız — boş sonuç dönülüyor");
            return Task.FromResult(new OcrResult(string.Empty, 0, Name));
        }
    }
}

/// <summary>
/// OCR henüz kurulmamış ortamlar için güvenli varsayılan: metin çıkarmaz,
/// güven skoru 0 döner — akış kırılmaz, belge depoda kalır.
/// </summary>
public sealed class StubOcrProvider : IOcrProvider
{
    public string Name => "Stub";

    public Task<OcrResult> ExtractTextAsync(byte[] content, CancellationToken cancellationToken = default) =>
        Task.FromResult(new OcrResult(string.Empty, 0, Name));
}
