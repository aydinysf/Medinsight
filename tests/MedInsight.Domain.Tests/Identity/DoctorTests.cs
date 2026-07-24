using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;
using MedInsight.Domain.Identity.Events;

namespace MedInsight.Domain.Tests.Identity;

public class DoctorTests
{
    private static Doctor NewDoctor(int capacity = 2) =>
        Doctor.Create(Guid.NewGuid(), "Nöroloji", "TR-12345", "Doç. Dr.", yearsOfExperience: 10, capacityThreshold: capacity);

    // --- Doğrulama (ADR-007) ---

    [Fact]
    public void Dogrulama_gonderimi_Pending_baslar_ve_event_uretir()
    {
        var doctor = NewDoctor();

        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "doctors/x/diploma.pdf", "QR-RAW", "{\"raw\":\"QR-RAW\"}");

        Assert.Equal(VerificationStatus.Pending, verification.Status);
        Assert.Equal(VerificationMethod.QrParsed, verification.Method);
        Assert.Contains(doctor.DomainEvents, e => e is DoctorVerificationSubmitted);
    }

    [Fact]
    public void QR_olmadan_gonderim_Manual_yontem_olur()
    {
        var doctor = NewDoctor();

        var verification = doctor.SubmitVerification(VerificationDocumentType.SpecialtyCertificate, "url", null, null);

        Assert.Equal(VerificationMethod.Manual, verification.Method);
    }

    [Fact]
    public void Admin_onayi_doktoru_Verified_yapar()
    {
        var doctor = NewDoctor();
        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "url", null, null);
        var adminId = Guid.NewGuid();

        doctor.ApproveVerification(verification.Id, adminId);

        Assert.Equal(VerificationStatus.Verified, doctor.VerificationStatus);
        Assert.Equal(adminId, verification.VerifiedByAdminId);
        Assert.Contains(doctor.DomainEvents, e => e is DoctorVerified v && v.VerifiedByAdminId == adminId);
    }

    [Fact]
    public void Sonuclanmis_dogrulama_tekrar_islenemez()
    {
        var doctor = NewDoctor();
        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "url", null, null);
        doctor.ApproveVerification(verification.Id, Guid.NewGuid());

        Assert.Throws<DomainException>(() => doctor.ApproveVerification(verification.Id, Guid.NewGuid()));
        Assert.Throws<DomainException>(() => doctor.RejectVerification(verification.Id, Guid.NewGuid(), "geç"));
    }

    [Fact]
    public void Ret_gerekce_tasir()
    {
        var doctor = NewDoctor();
        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "url", null, null);

        doctor.RejectVerification(verification.Id, Guid.NewGuid(), "Belge okunaksız");

        Assert.Equal(VerificationStatus.Rejected, doctor.VerificationStatus);
        Assert.Equal("Belge okunaksız", verification.RejectionReason);
    }

    [Fact]
    public void Dogrulanmis_doktor_yeni_dogrulama_gonderemez()
    {
        var doctor = NewDoctor();
        var verification = doctor.SubmitVerification(VerificationDocumentType.Diploma, "url", null, null);
        doctor.ApproveVerification(verification.Id, Guid.NewGuid());

        Assert.Throws<DomainException>(() => doctor.SubmitVerification(VerificationDocumentType.Diploma, "url2", null, null));
    }

    // --- Müsaitlik (ADR-009) ---

    [Fact]
    public void Kapasite_asilinca_ComputedStatus_Busy_olur()
    {
        var doctor = NewDoctor(capacity: 2);
        Assert.Equal(AvailabilityStatus.Available, doctor.EffectiveStatus);

        doctor.IncrementActiveCases();
        doctor.IncrementActiveCases();

        Assert.Equal(AvailabilityStatus.Busy, doctor.EffectiveStatus);
        Assert.Contains(doctor.DomainEvents, e => e is DoctorAvailabilityChanged c && c.NewStatus == AvailabilityStatus.Busy && c.ChangedBy == "System");
    }

    [Fact]
    public void ComputedStatus_asla_Away_uretmez()
    {
        var doctor = NewDoctor(capacity: 1);
        for (var i = 0; i < 100; i++)
        {
            doctor.IncrementActiveCases();
        }

        Assert.Equal(AvailabilityStatus.Busy, doctor.ComputedStatus);
    }

    [Fact]
    public void ManualOverride_sistem_hesabindan_agir_basar()
    {
        var doctor = NewDoctor(capacity: 2);

        doctor.SetManualOverride(AvailabilityStatus.Away, DateTime.UtcNow.AddDays(3));

        Assert.Equal(AvailabilityStatus.Away, doctor.EffectiveStatus);
        Assert.Contains(doctor.DomainEvents, e => e is DoctorAvailabilityChanged c && c.NewStatus == AvailabilityStatus.Away && c.ChangedBy == "Doctor");
    }

    [Fact]
    public void Suresi_dolan_override_yok_sayilir()
    {
        var doctor = NewDoctor();

        doctor.SetManualOverride(AvailabilityStatus.Away, DateTime.UtcNow.AddMilliseconds(-1));

        Assert.Equal(AvailabilityStatus.Available, doctor.EffectiveStatus);
    }

    [Fact]
    public void Override_kaldirilinca_sistem_hesabina_donulur()
    {
        var doctor = NewDoctor(capacity: 1);
        doctor.IncrementActiveCases();
        doctor.SetManualOverride(AvailabilityStatus.Available);

        doctor.SetManualOverride(null);

        Assert.Equal(AvailabilityStatus.Busy, doctor.EffectiveStatus);
    }

    [Fact]
    public void ReviewerProfile_duzeltme_orani_hesaplar()
    {
        var profile = ReviewerProfile.Create(Guid.NewGuid(), "Nöroloji", 10);

        profile.RecordReview(corrected: false);
        profile.RecordReview(corrected: true);
        profile.RecordReview(corrected: false);
        profile.RecordReview(corrected: true);

        Assert.Equal(4, profile.CaseReviewCount);
        Assert.Equal(0.5m, profile.CorrectionRate);
    }
}
