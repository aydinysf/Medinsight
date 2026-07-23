using System.Text;
using MedInsight.Application.Abstractions.TextExtraction;
using UglyToad.PdfPig;

namespace MedInsight.Infrastructure.TextExtraction;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public string? ExtractText(byte[] pdfContent)
    {
        try
        {
            using var document = PdfDocument.Open(pdfContent, new ParsingOptions { UseLenientParsing = true });
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            var text = builder.ToString().Trim();
            return text.Length == 0 ? null : text;
        }
        catch (Exception)
        {
            // Bozuk/şifreli PDF — metin çıkarılamadı; belge depoda kalır, akış kırılmaz.
            return null;
        }
    }
}
