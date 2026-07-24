using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class ConsultationTests
{
    private static Case CaseInDoctorReview(out Guid analysisId)
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");
        var document = medicalCase.AddDocument("rapor.pdf", Guid.NewGuid(), "key", "rapor.pdf", "application/pdf", 100, "h1");
        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);
        medicalCase.ScoreDocumentQuality(document.Id, 1m, new Dictionary<string, decimal>(), [], isSufficient: true);
        var analysis = medicalCase.AddAiAnalysis("m1", "p1", 0.8m, "Özet", "Mesaj",
            [new AiFindingInput("Bulgu", AiFindingSource.LLMTextAnalysis, document.Id)], []);
        analysisId = analysis.Id;
        return medicalCase;
    }

    private static Consultation StartConsultation(Case medicalCase, out Guid doctorId)
    {
        doctorId = Guid.NewGuid();
        return medicalCase.StartConsultation(doctorId, Guid.NewGuid());
    }

    [Fact]
    public void Konsultasyon_baslar_doktor_Contribute_uyesi_olur()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var doctorUserId = Guid.NewGuid();

        var consultation = medicalCase.StartConsultation(Guid.NewGuid(), doctorUserId);

        Assert.Equal(ConsultationStatus.Active, consultation.Status);
        var doctorMember = medicalCase.Members.Single(m => m.UserId == doctorUserId);
        Assert.Equal(CaseRole.Doctor, doctorMember.Role);
        Assert.Equal(PermissionLevel.Contribute, doctorMember.PermissionLevel);
        Assert.Contains(medicalCase.DomainEvents, e => e is ConsultationStarted);
    }

    [Fact]
    public void Ayni_doktorla_ikinci_aktif_konsultasyon_acilamaz()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out var doctorId);

        Assert.Throws<DomainException>(() => medicalCase.StartConsultation(doctorId, Guid.NewGuid()));

        medicalCase.CompleteConsultation(consultation.Id);
        var second = medicalCase.StartConsultation(doctorId, Guid.NewGuid());
        Assert.Equal(ConsultationStatus.Active, second.Status);
    }

    [Fact]
    public void Mesaj_eventi_icerik_tasimaz_gizlilik()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out _);

        var message = medicalCase.AddConsultationMessage(consultation.Id, Guid.NewGuid(), "Hassas klinik içerik");

        var sent = medicalCase.DomainEvents.OfType<ConsultationMessageSent>().Single();
        Assert.Equal(message.Id, sent.MessageId);
        Assert.DoesNotContain("Hassas", System.Text.Json.JsonSerializer.Serialize(sent));
    }

    [Fact]
    public void Tamamlanan_konsultasyona_mesaj_gonderilemez()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out _);
        medicalCase.CompleteConsultation(consultation.Id);

        Assert.Throws<DomainException>(() => medicalCase.AddConsultationMessage(consultation.Id, Guid.NewGuid(), "mesaj"));
    }

    [Fact]
    public void Klinik_notu_yalnizca_konsultasyon_doktoru_yazabilir()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out var doctorId);

        medicalCase.AddClinicalNote(consultation.Id, doctorId, "Not");
        Assert.Throws<DomainException>(() => medicalCase.AddClinicalNote(consultation.Id, Guid.NewGuid(), "Baskasi"));
    }

    [Fact]
    public void Tedavi_plani_zorunlu_snapshot_ve_gecisleri_tetikler_invariant_2()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out var doctorId);
        var snapshotsBefore = medicalCase.HealthRouteSnapshots.Count;

        var treatment = medicalCase.CreateTreatmentPlan(consultation.Id, doctorId, "İlaç tedavisi + 3 ay takip", new DateOnly(2026, 10, 25));

        Assert.Equal(CaseStatus.FollowUp, medicalCase.Status); // DoctorReview → Treatment → FollowUp (kontrol tarihi verildi)
        Assert.Equal(snapshotsBefore + 1, medicalCase.HealthRouteSnapshots.Count);
        var snapshot = medicalCase.HealthRouteSnapshots.OrderByDescending(s => s.VersionNumber).First();
        Assert.Equal(RouteTrigger.Doctor, snapshot.TriggeredBy);
        Assert.Equal(treatment.Id, snapshot.TriggerSourceId);
        Assert.Contains(medicalCase.DomainEvents, e => e is TreatmentPlanCreated);
    }

    [Fact]
    public void Kontrol_tarihi_yoksa_Treatment_durumunda_kalir()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out var doctorId);

        medicalCase.CreateTreatmentPlan(consultation.Id, doctorId, "Tedavi planı");

        Assert.Equal(CaseStatus.Treatment, medicalCase.Status);
    }

    [Fact]
    public void Tedavi_planini_baska_doktor_olusturamaz()
    {
        var medicalCase = CaseInDoctorReview(out _);
        var consultation = StartConsultation(medicalCase, out _);

        Assert.Throws<DomainException>(() => medicalCase.CreateTreatmentPlan(consultation.Id, Guid.NewGuid(), "Plan"));
    }

    [Fact]
    public void Analiz_incelemesi_kaydedilir_ve_event_uretir()
    {
        var medicalCase = CaseInDoctorReview(out var analysisId);
        var doctorId = Guid.NewGuid();

        medicalCase.ReviewAiAnalysis(analysisId, doctorId, AnalysisReviewDecision.Corrected, "Bulgu eksik yorumlanmış");

        var analysis = medicalCase.AiAnalyses.Single();
        Assert.Equal(AnalysisReviewDecision.Corrected, analysis.ReviewDecision);
        Assert.Contains(medicalCase.DomainEvents, e => e is AIAnalysisReviewed r && r.Decision == AnalysisReviewDecision.Corrected);
    }

    [Fact]
    public void Duzeltme_karari_not_olmadan_kaydedilemez()
    {
        var medicalCase = CaseInDoctorReview(out var analysisId);

        Assert.Throws<DomainException>(() =>
            medicalCase.ReviewAiAnalysis(analysisId, Guid.NewGuid(), AnalysisReviewDecision.Corrected, null));
    }

    [Fact]
    public void Analiz_iki_kez_incelenemez()
    {
        var medicalCase = CaseInDoctorReview(out var analysisId);
        medicalCase.ReviewAiAnalysis(analysisId, Guid.NewGuid(), AnalysisReviewDecision.Approved, null);

        Assert.Throws<DomainException>(() =>
            medicalCase.ReviewAiAnalysis(analysisId, Guid.NewGuid(), AnalysisReviewDecision.Approved, null));
    }

    [Fact]
    public void Escalation_onceligi_yukseltir_ve_event_uretir_ADR014()
    {
        var medicalCase = CaseInDoctorReview(out _);

        medicalCase.SuggestEscalation(EscalationReason.DoctorRequested);

        Assert.Equal(ReviewPriority.High, medicalCase.ReviewPriority);
        Assert.Contains(medicalCase.DomainEvents, e => e is EscalationSuggested s && s.Reason == EscalationReason.DoctorRequested);
    }

    [Fact]
    public void Kapali_vakada_konsultasyon_baslatilamaz()
    {
        var medicalCase = Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Vaka");

        Assert.Throws<DomainException>(() => medicalCase.StartConsultation(Guid.NewGuid(), Guid.NewGuid()));
    }
}
