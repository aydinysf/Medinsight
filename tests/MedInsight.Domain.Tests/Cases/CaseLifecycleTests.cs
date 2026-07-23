using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Domain.Tests.Cases;

public class CaseLifecycleTests
{
    private static Case NewCase() => Case.Create(Guid.NewGuid(), Guid.NewGuid(), "Baş ağrısı takibi", BodySystem.Neuro);

    private static Case CaseAt(CaseStatus status)
    {
        var medicalCase = NewCase();
        if (status == CaseStatus.Draft)
        {
            return medicalCase;
        }

        var document = medicalCase.AddDocument("MR raporu", Guid.NewGuid());
        if (status == CaseStatus.CollectingData)
        {
            return medicalCase;
        }

        medicalCase.ClassifyDocument(document.Id, DocumentType.TextualReport);
        medicalCase.ScoreDocumentQuality(document.Id, 1m, new Dictionary<string, decimal> { ["Completeness"] = 1m }, [], isSufficient: true);
        if (status == CaseStatus.AIAnalysis)
        {
            return medicalCase;
        }

        medicalCase.CompleteAiAnalysis();
        if (status == CaseStatus.DoctorReview)
        {
            return medicalCase;
        }

        medicalCase.BeginTreatment();
        if (status == CaseStatus.Treatment)
        {
            return medicalCase;
        }

        medicalCase.ScheduleFollowUp();
        if (status == CaseStatus.FollowUp)
        {
            return medicalCase;
        }

        medicalCase.Close();
        return medicalCase;
    }

    [Fact]
    public void Create_baslangic_durumu_Draft_ve_CaseCreated_uretir()
    {
        var medicalCase = NewCase();

        Assert.Equal(CaseStatus.Draft, medicalCase.Status);
        Assert.Equal(RiskLevel.Unknown, medicalCase.RiskLevel);
        Assert.Contains(medicalCase.DomainEvents, e => e is CaseCreated);
    }

    [Fact]
    public void Create_hastayi_Manage_yetkisiyle_uye_yapar()
    {
        var patientUserId = Guid.NewGuid();
        var medicalCase = Case.Create(Guid.NewGuid(), patientUserId, "Vaka");

        var member = Assert.Single(medicalCase.Members);
        Assert.Equal(patientUserId, member.UserId);
        Assert.Equal(CaseRole.Patient, member.Role);
        Assert.Equal(PermissionLevel.Manage, member.PermissionLevel);
    }

    [Fact]
    public void Ilk_belge_Draft_tan_CollectingData_ya_gecirir()
    {
        var medicalCase = NewCase();

        medicalCase.AddDocument("MR raporu", Guid.NewGuid());

        Assert.Equal(CaseStatus.CollectingData, medicalCase.Status);
        Assert.Contains(medicalCase.DomainEvents, e => e is DocumentUploaded);
        Assert.Contains(medicalCase.DomainEvents, e => e is CaseStatusChanged c && c.ToStatus == CaseStatus.CollectingData);
    }

    [Fact]
    public void Ikinci_belge_durum_degistirmez()
    {
        var medicalCase = CaseAt(CaseStatus.CollectingData);

        medicalCase.AddDocument("Lab sonucu", Guid.NewGuid());

        Assert.Equal(CaseStatus.CollectingData, medicalCase.Status);
    }

    [Fact]
    public void StartAiAnalysis_kaliteli_belge_yoksa_reddeder()
    {
        var medicalCase = CaseAt(CaseStatus.CollectingData);

        Assert.Throws<DomainException>(medicalCase.StartAiAnalysis);
    }

    [Fact]
    public void Mutlu_yol_tum_gecisleri_sirayla_yapar()
    {
        var medicalCase = CaseAt(CaseStatus.Closed);

        Assert.Equal(CaseStatus.Closed, medicalCase.Status);

        var transitions = medicalCase.DomainEvents.OfType<CaseStatusChanged>().Select(e => e.ToStatus).ToList();
        Assert.Equal(
            [CaseStatus.CollectingData, CaseStatus.AIAnalysis, CaseStatus.DoctorReview, CaseStatus.Treatment, CaseStatus.FollowUp, CaseStatus.Closed],
            transitions);
    }

    [Fact]
    public void Kapali_vakaya_belge_eklenemez_invariant_5()
    {
        var medicalCase = CaseAt(CaseStatus.Closed);

        Assert.Throws<DomainException>(() => medicalCase.AddDocument("Yeni rapor", Guid.NewGuid()));
    }

    [Fact]
    public void Reopen_Closed_dan_FollowUp_a_doner_Draft_a_degil()
    {
        var medicalCase = CaseAt(CaseStatus.Closed);

        medicalCase.Reopen("Hasta yeni semptom bildirdi");

        Assert.Equal(CaseStatus.FollowUp, medicalCase.Status);
        Assert.Contains(medicalCase.DomainEvents, e => e is CaseReopened);
    }

    [Fact]
    public void FollowUp_yeni_veri_ile_CollectingData_ya_doner()
    {
        var medicalCase = CaseAt(CaseStatus.FollowUp);

        medicalCase.ReceiveNewData("Yeni MR yüklendi");

        Assert.Equal(CaseStatus.CollectingData, medicalCase.Status);
    }

    [Theory]
    [InlineData(CaseStatus.Draft)]
    [InlineData(CaseStatus.CollectingData)]
    [InlineData(CaseStatus.AIAnalysis)]
    [InlineData(CaseStatus.Treatment)]
    public void Close_yalnizca_FollowUp_tan_yapilabilir(CaseStatus fromStatus)
    {
        var medicalCase = CaseAt(fromStatus);

        Assert.Throws<DomainException>(medicalCase.Close);
    }

    [Fact]
    public void Gecersiz_gecis_DomainException_firlatir()
    {
        var medicalCase = NewCase();

        Assert.Throws<DomainException>(medicalCase.CompleteAiAnalysis);
        Assert.Throws<DomainException>(medicalCase.BeginTreatment);
        Assert.Throws<DomainException>(() => medicalCase.Reopen("olmaz"));
    }

    [Fact]
    public void Her_gecis_CaseStatusChanged_uretir_sessiz_gecis_yok()
    {
        var medicalCase = CaseAt(CaseStatus.Closed);

        var statusChanges = medicalCase.DomainEvents.OfType<CaseStatusChanged>().Count();
        Assert.Equal(6, statusChanges);
    }

    [Fact]
    public void DequeueDomainEvents_kuyrugu_bosaltir()
    {
        var medicalCase = NewCase();

        var events = medicalCase.DequeueDomainEvents();

        Assert.NotEmpty(events);
        Assert.Empty(medicalCase.DomainEvents);
    }

    [Fact]
    public void AddMember_ayni_kullaniciyi_iki_kez_eklemez()
    {
        var medicalCase = NewCase();
        var userId = Guid.NewGuid();

        medicalCase.AddMember(userId, CaseRole.Caregiver, PermissionLevel.Contribute);

        Assert.Throws<DomainException>(() => medicalCase.AddMember(userId, CaseRole.Caregiver, PermissionLevel.ReadOnly));
    }

    [Fact]
    public void Event_zarfi_caseId_ve_correlation_tasir()
    {
        var medicalCase = NewCase();
        var created = medicalCase.DomainEvents.OfType<CaseCreated>().Single();

        Assert.Equal(medicalCase.Id, created.CaseId);
        Assert.NotEqual(Guid.Empty, created.EventId);
        Assert.NotEqual(Guid.Empty, created.CorrelationId);
        Assert.Equal(nameof(CaseCreated), created.EventType);
    }
}
