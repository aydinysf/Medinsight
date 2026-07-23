using System.Text;
using MedInsight.Application.Ingestion;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Tests.Ingestion;

public class DocumentClassifierTests
{
    private static byte[] DicomBytes()
    {
        var bytes = new byte[200];
        bytes[128] = (byte)'D';
        bytes[129] = (byte)'I';
        bytes[130] = (byte)'C';
        bytes[131] = (byte)'M';
        return bytes;
    }

    [Fact]
    public void Dicom_magic_number_DicomFile()
    {
        Assert.Equal(DocumentType.DicomFile, DocumentClassifier.Classify("goruntu.bin", "application/octet-stream", DicomBytes()));
    }

    [Fact]
    public void Dcm_uzantisi_DicomFile()
    {
        Assert.Equal(DocumentType.DicomFile, DocumentClassifier.Classify("seri001.dcm", "application/octet-stream", new byte[10]));
    }

    [Fact]
    public void Metin_katmanli_pdf_TextualReport()
    {
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.7 ... /Type /Font /BaseFont /Helvetica ...");

        Assert.Equal(DocumentType.TextualReport, DocumentClassifier.Classify("rapor.pdf", "application/pdf", pdf));
    }

    [Fact]
    public void Metin_katmansiz_pdf_ScannedReport()
    {
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.7 ... /Type /XObject /Subtype /Image ...");

        Assert.Equal(DocumentType.ScannedReport, DocumentClassifier.Classify("tarama.pdf", "application/pdf", pdf));
    }

    [Fact]
    public void Pdf_uzantili_ama_pdf_olmayan_dosya_siniflandirilamaz()
    {
        Assert.Null(DocumentClassifier.Classify("sahte.pdf", "application/pdf", Encoding.ASCII.GetBytes("bu bir pdf degil")));
    }

    [Theory]
    [InlineData("lezyon.jpg", "image/jpeg")]
    [InlineData("cilt.png", "image/png")]
    public void Goruntu_dosyalari_PhotoDocument(string fileName, string contentType)
    {
        Assert.Equal(DocumentType.PhotoDocument, DocumentClassifier.Classify(fileName, contentType, new byte[10]));
    }

    [Fact]
    public void Bilinmeyen_tur_null()
    {
        Assert.Null(DocumentClassifier.Classify("video.mp4", "video/mp4", new byte[10]));
    }
}
