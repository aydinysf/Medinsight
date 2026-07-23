using MedInsight.Application.Quality;
using MedInsight.Application.Quality.Criteria;
using MedInsight.Application.Tests.Fakes;
using MedInsight.Domain.Cases;
using Microsoft.Extensions.Options;

namespace MedInsight.Application.Tests.Quality;

public class QualityEngineTests
{
    private readonly FakeDicomReader _dicomReader = new();

    private QualityEngine Engine(decimal threshold = 0.7m, Dictionary<string, decimal>? weights = null, FakeOcrProvider? ocr = null) =>
        new(
            [
                new DuplicatedFilesCriterion(),
                new CompletenessCriterion(),
                new DicomIntegrityCriterion(_dicomReader),
                new OcrScoreCriterion(ocr ?? new FakeOcrProvider("Stub")),
            ],
            Options.Create(new QualityOptions { SufficientScore = threshold, Weights = weights ?? [] }));

    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

    private static MedicalDocument AddDoc(Case medicalCase, string hash, DocumentType type = DocumentType.TextualReport, string name = "rapor.pdf")
    {
        var document = medicalCase.AddDocument(name, Guid.NewGuid(), "key", name, "application/pdf", 100, hash);
        medicalCase.ClassifyDocument(document.Id, type);
        return document;
    }

    [Fact]
    public async Task Saglikli_belge_yeterli_skor_alir()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash1");

        var report = await Engine().EvaluateAsync(new QualityContext(medicalCase, document, new byte[100]));

        Assert.True(report.IsSufficient);
        Assert.Equal(1m, report.OverallScore);
        Assert.Empty(report.FailureReasons);
    }

    [Fact]
    public async Task Ayni_icerik_tekrar_yuklenirse_DuplicatedFiles_dusurur()
    {
        var medicalCase = NewCase();
        AddDoc(medicalCase, "hash1", name: "ilk.pdf");
        var duplicate = AddDoc(medicalCase, "hash1", name: "kopya.pdf");

        var report = await Engine().EvaluateAsync(new QualityContext(medicalCase, duplicate, new byte[100]));

        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("DuplicatedFiles"));
    }

    [Fact]
    public async Task Bos_dosya_Completeness_ten_kalir()
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("bos.pdf", Guid.NewGuid(), "key", "bos.pdf", "application/pdf", 0, "hash-empty");
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);

        var report = await Engine().EvaluateAsync(new QualityContext(medicalCase, document, []));

        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("Completeness"));
    }

    [Fact]
    public async Task Zorunlu_dicom_alanlari_eksikse_DicomIntegrity_dusurur()
    {
        _dicomReader.Integrity = new(HasPatientId: true, HasStudyDate: false, HasModality: false);
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash-dcm", DocumentType.DicomFile, "seri.dcm");

        // Doküman: DICOM çalışmalarında DicomIntegrity en yüksek ağırlığı taşır
        var report = await Engine(weights: new Dictionary<string, decimal> { ["DicomIntegrity"] = 3m })
            .EvaluateAsync(new QualityContext(medicalCase, document, new byte[100]));

        Assert.Contains(report.FailureReasons, r => r.Contains("StudyDate") && r.Contains("Modality"));
        Assert.False(report.IsSufficient);
    }

    [Fact]
    public async Task OcrScore_stub_saglayicida_uygulanmaz()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash-scan", DocumentType.ScannedReport, "tarama.pdf");

        var report = await Engine(ocr: new FakeOcrProvider("Stub")).EvaluateAsync(new QualityContext(medicalCase, document, new byte[100]));

        Assert.DoesNotContain("OcrScore", report.CriteriaScores.Keys);
        Assert.True(report.IsSufficient);
    }

    [Fact]
    public async Task Dusuk_ocr_guveni_taranan_belgeyi_dusurur()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash-scan", DocumentType.ScannedReport, "tarama.pdf");

        // Doküman: taranan PDF'te OCR Score öncelikli ağırlık taşır
        var report = await Engine(weights: new Dictionary<string, decimal> { ["OcrScore"] = 2m }, ocr: new FakeOcrProvider("Tesseract", "kismi metin", 0.2m))
            .EvaluateAsync(new QualityContext(medicalCase, document, new byte[100]));

        Assert.Contains("OcrScore", report.CriteriaScores.Keys);
        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("OcrScore"));
    }

    [Fact]
    public async Task Agirliklar_konfigurasyondan_okunur()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash-scan", DocumentType.ScannedReport, "tarama.pdf");

        // OCR güveni düşük ama ağırlığı 0 → sonucu etkilemez
        var report = await Engine(weights: new Dictionary<string, decimal> { ["OcrScore"] = 0m }, ocr: new FakeOcrProvider("Tesseract", "", 0.1m))
            .EvaluateAsync(new QualityContext(medicalCase, document, new byte[100]));

        Assert.True(report.IsSufficient);
        Assert.Equal(1m, report.OverallScore);
    }
}
