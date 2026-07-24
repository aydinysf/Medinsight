using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class ImageFindingTests
{
    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

    [Fact]
    public void Goruntu_bulgusu_eklenir_ve_event_uretir()
    {
        var medicalCase = NewCase();
        var studyId = Guid.NewGuid();

        var finding = medicalCase.AddImageFinding(
            studyId, "nnUNet-BraTS-v1", "OpenSource", "Segmentation",
            "Sol frontal bölgede segmentasyon maskesi üretildi.",
            "{\"volumeMm3\": 2300}",
            "Bu bulgu klinik olarak doğrulanmamış bir model tarafından üretilmiştir.");

        Assert.Single(medicalCase.ImageFindings);
        Assert.Equal(studyId, finding.StudyId);
        Assert.Contains(medicalCase.DomainEvents, e => e is ImageFindingAdded a && a.FindingId == finding.Id);
    }

    [Fact]
    public void Disclaimersiz_goruntu_bulgusu_eklenemez_ADR010()
    {
        var medicalCase = NewCase();

        Assert.Throws<DomainException>(() => medicalCase.AddImageFinding(
            null, "nnUNet-BraTS-v1", "OpenSource", "Segmentation", "Bulgu", "{}", disclaimer: "  "));
    }

    [Fact]
    public void Goruntu_bulgusu_DifferentialDiagnosis_tarafindan_referans_alinamaz()
    {
        // Yapısal izolasyon: DifferentialDiagnosis yalnızca kendi analizindeki
        // AiFinding'lere indeksle referans verebilir; ImageFinding ayrı tipte ve
        // ayrı koleksiyonda yaşadığı için derleme düzeyinde bile erişim yolu yoktur.
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("rapor.pdf", Guid.NewGuid(), "key", "rapor.pdf", "application/pdf", 100, "h");
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);
        medicalCase.ScoreDocumentQuality(document.Id, 1m, new Dictionary<string, decimal>(), [], true);
        medicalCase.AddImageFinding(null, "m", "OpenSource", "Classification", "Bulgu", "{}", "Doğrulanmamış model çıktısı.");

        // Analiz, görüntü bulgusuna işaret edemez — geçersiz indeks reddedilir.
        Assert.Throws<DomainException>(() => medicalCase.AddAiAnalysis(
            "m1", "p1", 0.8m, "Özet", "Mesaj",
            [],
            [new DifferentialDiagnosisInput("Aday", 0.7m, RiskLevel.Low, [0])]));
    }
}
