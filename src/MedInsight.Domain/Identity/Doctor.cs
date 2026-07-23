using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public sealed class Doctor : Entity
{
    private Doctor()
    {
    }

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public string Specialty { get; private set; } = null!;

    public string LicenseNumber { get; private set; } = null!;

    public VerificationStatus VerificationStatus { get; private set; }

    public static Doctor Create(Guid userId, string specialty, string licenseNumber, string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseNumber);

        return new Doctor
        {
            UserId = userId,
            Title = title,
            Specialty = specialty.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            VerificationStatus = VerificationStatus.Pending,
        };
    }

    public void MarkVerified() => VerificationStatus = VerificationStatus.Verified;

    public void MarkRejected() => VerificationStatus = VerificationStatus.Rejected;
}

/// <summary>Son onaylı DoctorVerification kaydının özeti (bkz. docs/domain/doctor-verification.md).</summary>
public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
}
