using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class DocumentIngestionTests
{
    private static Case NewCaseWithDocument(out MedicalDocument document)
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");
        document = medicalCase.AddDocument("rapor.pdf", Guid.NewGuid(), "key", "rapor.pdf", "application/pdf", 100, "hash1");
        return medicalCase;
    }

    [Fact]
    public void ClassifyDocument_status_ve_tip_gunceller_event_uretir()
    {
        var medicalCase = NewCaseWithDocument(out var document);

        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);

        Assert.Equal(DocumentType.TextualReport, document.Type);
        Assert.Equal(DocumentStatus.Classified, document.Status);
        Assert.Contains(medicalCase.DomainEvents, e => e is DocumentClassified c && c.DocumentId == document.Id);
    }

    [Fact]
    public void Siniflandirilamayan_dosya_sessizce_yok_sayilmaz()
    {
        var medicalCase = NewCaseWithDocument(out var document);

        medicalCase.MarkDocumentClassificationFailed(document.Id, "Tür tanınamadı");

        Assert.Equal(DocumentStatus.ClassificationFailed, document.Status);
        Assert.Contains(medicalCase.DomainEvents, e => e is DocumentClassificationFailed f && f.Reason == "Tür tanınamadı");
    }

    [Fact]
    public void Yeterli_kalite_skoru_belgeyi_hazirlar_ve_AIAnalysis_e_gecirir()
    {
        var medicalCase = NewCaseWithDocument(out var document);
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);

        medicalCase.ScoreDocumentQuality(document.Id, 0.9m, new Dictionary<string, decimal>(), [], isSufficient: true);

        Assert.Equal(DocumentStatus.QualityChecked, document.Status);
        Assert.Equal(CaseStatus.AIAnalysis, medicalCase.Status);
        Assert.Contains(medicalCase.DomainEvents, e => e is DocumentQualityScored q && q.IsSufficient);
    }

    [Fact]
    public void Yetersiz_kalite_skoru_belgeyi_reddeder_durum_degismez()
    {
        var medicalCase = NewCaseWithDocument(out var document);
        medicalCase.ClassifyDocument(document.Id, DocumentType.ScannedReport);

        medicalCase.ScoreDocumentQuality(document.Id, 0.3m, new Dictionary<string, decimal>(), ["OCR skoru düşük"], isSufficient: false);

        Assert.Equal(DocumentStatus.Rejected, document.Status);
        Assert.Equal(CaseStatus.CollectingData, medicalCase.Status);
    }

    [Fact]
    public void Olmayan_belge_icin_DomainException()
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

        Assert.Throws<DomainException>(() => medicalCase.ClassifyDocument(Guid.NewGuid(), DocumentType.PhotoDocument));
    }
}
