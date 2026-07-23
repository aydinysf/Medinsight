using MedInsight.Application.Quality;
using MedInsight.Application.Quality.Criteria;
using MedInsight.Domain.Cases;
using Microsoft.Extensions.Options;

namespace MedInsight.Application.Tests.Quality;

public class QualityEngineTests
{
    private static QualityEngine Engine(decimal threshold = 0.7m) =>
        new(
            [new DuplicatedFilesCriterion(), new CompletenessCriterion(), new DicomIntegrityCriterion()],
            Options.Create(new QualityOptions { SufficientScore = threshold }));

    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

    private static MedicalDocument AddDoc(Case medicalCase, string hash, string name = "rapor.pdf")
    {
        var document = medicalCase.AddDocument(name, Guid.NewGuid(), "key", name, "application/pdf", 100, hash);
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);
        return document;
    }

    [Fact]
    public void Saglikli_belge_yeterli_skor_alir()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash1");

        var report = Engine().Evaluate(new QualityContext(medicalCase, document, new byte[100]));

        Assert.True(report.IsSufficient);
        Assert.Equal(1m, report.OverallScore);
        Assert.Empty(report.FailureReasons);
    }

    [Fact]
    public void Ayni_iceriktekrar_yuklenirse_DuplicatedFiles_dusurur()
    {
        var medicalCase = NewCase();
        AddDoc(medicalCase, "hash1", "ilk.pdf");
        var duplicate = AddDoc(medicalCase, "hash1", "kopya.pdf");

        var report = Engine().Evaluate(new QualityContext(medicalCase, duplicate, new byte[100]));

        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("DuplicatedFiles"));
    }

    [Fact]
    public void Bos_dosya_Completeness_ten_kalir()
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("bos.pdf", Guid.NewGuid(), "key", "bos.pdf", "application/pdf", 0, "hash-empty");
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);

        var report = Engine().Evaluate(new QualityContext(medicalCase, document, []));

        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("Completeness"));
    }

    [Fact]
    public void Dicom_kriteri_yalnizca_dicom_belgesine_uygulanir()
    {
        var medicalCase = NewCase();
        var document = AddDoc(medicalCase, "hash1");

        var report = Engine().Evaluate(new QualityContext(medicalCase, document, new byte[100]));

        Assert.DoesNotContain("DicomIntegrity", report.CriteriaScores.Keys);
    }

    [Fact]
    public void Dicm_basligi_olmayan_dicom_kalir()
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("seri.dcm", Guid.NewGuid(), "key", "seri.dcm", "application/octet-stream", 100, "hash-dcm");
        medicalCase.ClassifyDocument(document.Id, DocumentType.DicomFile);

        var report = Engine().Evaluate(new QualityContext(medicalCase, document, new byte[100]));

        Assert.False(report.IsSufficient);
        Assert.Contains(report.FailureReasons, r => r.StartsWith("DicomIntegrity"));
    }
}
