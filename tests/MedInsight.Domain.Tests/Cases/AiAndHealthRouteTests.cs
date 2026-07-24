using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class AiAndHealthRouteTests
{
    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

    private static Case CaseInAiAnalysis(out Guid documentId)
    {
        var medicalCase = NewCase();
        var document = medicalCase.AddDocument("rapor.pdf", Guid.NewGuid(), "key", "rapor.pdf", "application/pdf", 100, "h1");
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);
        medicalCase.ScoreDocumentQuality(document.Id, 1m, new Dictionary<string, decimal>(), [], isSufficient: true);
        documentId = document.Id;
        return medicalCase;
    }

    [Fact]
    public void Vaka_acilinca_ilk_rota_snapshoti_olusur_ADR002()
    {
        var medicalCase = NewCase();

        Assert.NotNull(medicalCase.HealthRoute);
        var snapshot = Assert.Single(medicalCase.HealthRouteSnapshots);
        Assert.Equal(1, snapshot.VersionNumber);
        Assert.Null(snapshot.PreviousVersionId);
        Assert.Equal(RouteTrigger.System, snapshot.TriggeredBy);
        Assert.Equal(snapshot.Id, medicalCase.HealthRoute.CurrentVersionId);
        Assert.Contains(medicalCase.DomainEvents, e => e is HealthRouteSnapshotCreated);
    }

    [Fact]
    public void UpdateHealthRoute_dogrusal_zincir_kurar()
    {
        var medicalCase = NewCase();
        var v1 = medicalCase.HealthRouteSnapshots.Single();

        var v2 = medicalCase.UpdateHealthRoute("AI analizi tamam", "Doktor incelemesi", RiskLevel.Medium, RouteTrigger.AI, Guid.NewGuid(), "analiz");

        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(v1.Id, v2.PreviousVersionId);
        Assert.Equal(v2.Id, medicalCase.HealthRoute!.CurrentVersionId);
        Assert.Equal(RiskLevel.Medium, medicalCase.HealthRoute.RiskLevel);
        Assert.Equal(2, medicalCase.HealthRouteSnapshots.Count);
    }

    [Fact]
    public void RequestAiAnalysis_yalnizca_AIAnalysis_durumunda_event_uretir()
    {
        var medicalCase = CaseInAiAnalysis(out var documentId);

        medicalCase.RequestAiAnalysis();

        var requested = medicalCase.DomainEvents.OfType<AIAnalysisRequested>().Single();
        Assert.Contains(documentId, requested.DocumentIds);
    }

    [Fact]
    public void RequestAiAnalysis_diger_durumlarda_sessizce_atlanir()
    {
        var medicalCase = NewCase();

        medicalCase.RequestAiAnalysis();

        Assert.DoesNotContain(medicalCase.DomainEvents, e => e is AIAnalysisRequested);
    }

    [Fact]
    public void AddAiAnalysis_analizi_kaydeder_ve_DoctorReview_e_gecirir()
    {
        var medicalCase = CaseInAiAnalysis(out var documentId);

        var analysis = medicalCase.AddAiAnalysis(
            "model-1", "prompt-1", 0.8m, "Özet", "Hasta mesajı",
            [new AiFindingInput("Bulgu", AiFindingSource.LLMTextAnalysis, documentId)],
            [new DifferentialDiagnosisInput("Aday değerlendirme", 0.7m, RiskLevel.Low, [0])]);

        Assert.Equal(CaseStatus.DoctorReview, medicalCase.Status);
        Assert.Single(medicalCase.AiAnalyses);
        Assert.Single(analysis.Findings);
        var differential = Assert.Single(analysis.DifferentialDiagnoses);
        Assert.Equal(analysis.Findings.Single().Id, differential.SourceFindingIds.Single());
        Assert.Contains(medicalCase.DomainEvents, e => e is AIAnalysisCompleted c && c.AnalysisId == analysis.Id);
    }

    [Fact]
    public void Kaynaksiz_tani_adayi_eklenemez_invariant_3()
    {
        var medicalCase = CaseInAiAnalysis(out var documentId);

        Assert.Throws<DomainException>(() => medicalCase.AddAiAnalysis(
            "model-1", "prompt-1", 0.8m, "Özet", "Mesaj",
            [new AiFindingInput("Bulgu", AiFindingSource.LLMTextAnalysis, documentId)],
            [new DifferentialDiagnosisInput("Kaynaksız", 0.7m, RiskLevel.Low, [])]));
    }

    [Fact]
    public void OpenSource_bulgu_tani_adayini_besleyemez_ADR010()
    {
        var medicalCase = CaseInAiAnalysis(out var documentId);

        Assert.Throws<DomainException>(() => medicalCase.AddAiAnalysis(
            "model-1", "prompt-1", 0.8m, "Özet", "Mesaj",
            [new AiFindingInput("Segmentasyon", AiFindingSource.OpenSourceImageModel, documentId, "Doğrulanmamış model çıktısı")],
            [new DifferentialDiagnosisInput("Aday", 0.7m, RiskLevel.Low, [0])]));
    }

    [Fact]
    public void OpenSource_bulgu_disclaimersiz_eklenemez_ADR010()
    {
        var medicalCase = CaseInAiAnalysis(out var documentId);

        Assert.Throws<DomainException>(() => medicalCase.AddAiAnalysis(
            "model-1", "prompt-1", 0.8m, "Özet", "Mesaj",
            [new AiFindingInput("Segmentasyon", AiFindingSource.OpenSourceImageModel, documentId, Disclaimer: null)],
            []));
    }

    [Fact]
    public void AIAnalysis_durumunda_olmayan_vakaya_analiz_eklenemez()
    {
        var medicalCase = NewCase();

        Assert.Throws<DomainException>(() => medicalCase.AddAiAnalysis("m", "p", 0.5m, "Özet", "Mesaj", [], []));
    }

    [Fact]
    public void EscalateReviewPriority_oncelik_yukseltir_ve_event_uretir_ADR004()
    {
        var medicalCase = NewCase();
        var analysisId = Guid.NewGuid();

        medicalCase.EscalateReviewPriority(analysisId, "Düşük güven skoru");

        Assert.Equal(ReviewPriority.High, medicalCase.ReviewPriority);
        Assert.Contains(medicalCase.DomainEvents, e => e is DoctorReviewPriorityRaised p && p.AnalysisId == analysisId);
    }
}
