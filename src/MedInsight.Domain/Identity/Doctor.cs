using MedInsight.Domain.Common;
using MedInsight.Domain.Identity.Events;

namespace MedInsight.Domain.Identity;

public enum AvailabilityStatus
{
    Available = 0,
    Busy = 1,
    Away = 2,
}

public sealed class Doctor : AggregateRoot
{
    private readonly List<DoctorVerification> _verifications = [];

    private Doctor()
    {
    }

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public string Specialty { get; private set; } = null!;

    public string LicenseNumber { get; private set; } = null!;

    public int YearsOfExperience { get; private set; }

    public VerificationStatus VerificationStatus { get; private set; }

    // --- Müsaitlik (ADR-009): EffectiveStatus = ManualOverride ?? ComputedStatus ---

    public int ActiveCaseCount { get; private set; }

    public int CapacityThreshold { get; private set; }

    public AvailabilityStatus? ManualOverride { get; private set; }

    public DateTime? OverrideExpiresAt { get; private set; }

    public IReadOnlyCollection<DoctorVerification> Verifications => _verifications.AsReadOnly();

    /// <summary>ComputedStatus asla Away üretemez — tam kapanma yalnızca doktorun bilinçli kararıdır.</summary>
    public AvailabilityStatus ComputedStatus =>
        ActiveCaseCount >= CapacityThreshold ? AvailabilityStatus.Busy : AvailabilityStatus.Available;

    public AvailabilityStatus EffectiveStatus =>
        ManualOverride is not null && (OverrideExpiresAt is null || OverrideExpiresAt > DateTime.UtcNow)
            ? ManualOverride.Value
            : ComputedStatus;

    public static Doctor Create(Guid userId, string specialty, string licenseNumber, string? title = null, int yearsOfExperience = 0, int capacityThreshold = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseNumber);

        return new Doctor
        {
            UserId = userId,
            Title = title,
            Specialty = specialty.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            YearsOfExperience = Math.Max(0, yearsOfExperience),
            VerificationStatus = VerificationStatus.Pending,
            CapacityThreshold = Math.Max(1, capacityThreshold),
        };
    }

    // --- Doğrulama (ADR-007) ---

    public DoctorVerification SubmitVerification(VerificationDocumentType documentType, string documentUrl, string? qrPayload, string? qrParsedData)
    {
        if (VerificationStatus == VerificationStatus.Verified)
        {
            throw new DomainException("Doktor zaten doğrulanmış.");
        }

        var verification = DoctorVerification.Create(Id, documentType, documentUrl, qrPayload, qrParsedData);
        _verifications.Add(verification);
        VerificationStatus = VerificationStatus.Pending;
        Raise(new DoctorVerificationSubmitted { VerificationId = verification.Id, DoctorId = Id, DocumentType = documentType });
        return verification;
    }

    /// <summary>Admin onayı zorunlu — otomatik onay yolu yoktur (ADR-007).</summary>
    public void ApproveVerification(Guid verificationId, Guid adminId)
    {
        var verification = GetVerification(verificationId);
        verification.Approve(adminId);
        VerificationStatus = VerificationStatus.Verified;
        Raise(new DoctorVerified { VerificationId = verificationId, DoctorId = Id, VerifiedByAdminId = adminId });
    }

    public void RejectVerification(Guid verificationId, Guid adminId, string reason)
    {
        var verification = GetVerification(verificationId);
        verification.Reject(adminId, reason);
        VerificationStatus = VerificationStatus.Rejected;
        Raise(new DoctorVerificationRejected { VerificationId = verificationId, DoctorId = Id, VerifiedByAdminId = adminId, RejectionReason = reason });
    }

    // --- Müsaitlik operasyonları (ADR-009) ---

    /// <summary>Doktorun sözü sistemin hesabından ağır basar; null = override kaldır.</summary>
    public void SetManualOverride(AvailabilityStatus? status, DateTime? expiresAt = null)
    {
        var previous = EffectiveStatus;
        ManualOverride = status;
        OverrideExpiresAt = status is null ? null : expiresAt;

        if (EffectiveStatus != previous)
        {
            Raise(new DoctorAvailabilityChanged
            {
                DoctorId = Id,
                PreviousStatus = previous,
                NewStatus = EffectiveStatus,
                ChangedBy = "Doctor",
                OverrideExpiresAt = OverrideExpiresAt,
            });
        }
    }

    public void IncrementActiveCases() => ChangeActiveCases(+1);

    public void DecrementActiveCases() => ChangeActiveCases(-1);

    private void ChangeActiveCases(int delta)
    {
        var previous = EffectiveStatus;
        ActiveCaseCount = Math.Max(0, ActiveCaseCount + delta);

        if (EffectiveStatus != previous)
        {
            Raise(new DoctorAvailabilityChanged
            {
                DoctorId = Id,
                PreviousStatus = previous,
                NewStatus = EffectiveStatus,
                ChangedBy = "System",
            });
        }
    }

    private DoctorVerification GetVerification(Guid verificationId) =>
        _verifications.FirstOrDefault(v => v.Id == verificationId)
            ?? throw new DomainException("Doğrulama kaydı bulunamadı.");
}

/// <summary>Son onaylı DoctorVerification kaydının özeti (bkz. docs/domain/doctor-verification.md).</summary>
public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
}
