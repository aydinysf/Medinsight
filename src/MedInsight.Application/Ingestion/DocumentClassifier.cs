using System.Text;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Ingestion;

/// <summary>
/// Kural bazlı, deterministik sınıflandırma — AI kullanılmaz
/// (bkz. docs/architecture/ingestion-pipeline.md, 1. Classification).
/// </summary>
public static class DocumentClassifier
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".heic", ".webp"];

    /// <summary>Sınıflandırılamıyorsa null döner (DocumentClassificationFailed yolu).</summary>
    public static DocumentType? Classify(string? fileName, string? contentType, ReadOnlySpan<byte> content)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        if (extension == ".dcm" || HasDicomMagicNumber(content))
        {
            return DocumentType.DicomFile;
        }

        if (extension == ".pdf" || contentType == "application/pdf")
        {
            if (!IsPdf(content))
            {
                return null;
            }

            return HasPdfTextLayer(content) ? DocumentType.TextualReport : DocumentType.ScannedReport;
        }

        if (ImageExtensions.Contains(extension) || contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
        {
            return DocumentType.PhotoDocument;
        }

        return null;
    }

    /// <summary>DICOM preamble: 128 bayt + "DICM".</summary>
    private static bool HasDicomMagicNumber(ReadOnlySpan<byte> content) =>
        content.Length >= 132
        && content[128] == (byte)'D' && content[129] == (byte)'I'
        && content[130] == (byte)'C' && content[131] == (byte)'M';

    private static bool IsPdf(ReadOnlySpan<byte> content) =>
        content.Length >= 5
        && content[0] == (byte)'%' && content[1] == (byte)'P'
        && content[2] == (byte)'D' && content[3] == (byte)'F';

    /// <summary>
    /// Metin katmanı sezgisel kontrolü: font tanımı içeren PDF'te metin katmanı vardır.
    /// Gerçek PDF ayrıştırıcı Text Extraction Service diliminde gelecek (ADR-011).
    /// </summary>
    private static bool HasPdfTextLayer(ReadOnlySpan<byte> content)
    {
        var searchWindow = content.Length > 65536 ? content[..65536] : content;
        var text = Encoding.ASCII.GetString(searchWindow);
        return text.Contains("/Font", StringComparison.Ordinal);
    }
}
