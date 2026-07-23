using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class DicomGroupingAndRoutingTests
{
    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

    [Fact]
    public void RegisterDicomFile_ayni_study_uid_tek_calisma_olusturur()
    {
        var medicalCase = NewCase();

        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        medicalCase.RegisterDicomFile("study-1", "series-2", Modality.MR);

        var study = Assert.Single(medicalCase.DicomStudies);
        Assert.Equal(2, study.Series.Count);
        Assert.Equal(2, study.Series.First(s => s.SeriesInstanceUid == "series-1").SliceCount);
        Assert.Equal(1, study.Series.First(s => s.SeriesInstanceUid == "series-2").SliceCount);
    }

    [Fact]
    public void Farkli_study_uid_ayri_calisma_olusturur()
    {
        var medicalCase = NewCase();

        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        medicalCase.RegisterDicomFile("study-2", "series-1", Modality.CT);

        Assert.Equal(2, medicalCase.DicomStudies.Count);
    }

    [Fact]
    public void CompleteDicomGrouping_event_uretir_ve_seri_ozetini_tasir()
    {
        var medicalCase = NewCase();
        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        var study = medicalCase.DicomStudies.Single();

        medicalCase.CompleteDicomGrouping(study.Id);

        Assert.True(study.IsGrouped);
        var grouped = medicalCase.DomainEvents.OfType<DicomStudyGrouped>().Single();
        var series = Assert.Single(grouped.SeriesList);
        Assert.Equal(2, series.SliceCount);
        Assert.Equal(Modality.MR, series.Modality);
    }

    [Fact]
    public void Ayni_calisma_iki_kez_gruplanamaz()
    {
        var medicalCase = NewCase();
        medicalCase.RegisterDicomFile("study-1", "series-1", Modality.MR);
        var study = medicalCase.DicomStudies.Single();
        medicalCase.CompleteDicomGrouping(study.Id);

        Assert.Throws<DomainException>(() => medicalCase.CompleteDicomGrouping(study.Id));
    }

    [Theory]
    [InlineData(DocumentType.TextualReport, DocumentRoute.TextExtraction)]
    [InlineData(DocumentType.ScannedReport, DocumentRoute.TextExtraction)]
    [InlineData(DocumentType.DicomFile, DocumentRoute.RadiologyInference)]
    [InlineData(DocumentType.PhotoDocument, DocumentRoute.StorageOnly)]
    public void DecideRouting_dokumandaki_tabloya_uyar(DocumentType type, DocumentRoute expected)
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("dosya", Guid.NewGuid(), "key", "dosya", "application/octet-stream", 10, "h");
        medicalCase.ClassifyDocument(document.Id, type);
        medicalCase.ScoreDocumentQuality(document.Id, 1m, new Dictionary<string, decimal>(), [], isSufficient: true);

        var route = medicalCase.DecideRouting(document.Id);

        Assert.Equal(expected, route);
        Assert.Contains(medicalCase.DomainEvents, e => e is RoutingDecided r && r.Route == expected);
    }

    [Fact]
    public void Kaliteden_gecmemis_belge_yonlendirilemez()
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("dosya", Guid.NewGuid());

        Assert.Throws<DomainException>(() => medicalCase.DecideRouting(document.Id));
    }

    [Fact]
    public void StoreExtractedText_belgeye_metin_ve_guven_yazar()
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("rapor.pdf", Guid.NewGuid());

        medicalCase.StoreExtractedText(document.Id, "MR raporu bulgular ...", 0.87m);

        Assert.Equal("MR raporu bulgular ...", document.ExtractedText);
        Assert.Equal(0.87m, document.OcrConfidence);
    }
}
