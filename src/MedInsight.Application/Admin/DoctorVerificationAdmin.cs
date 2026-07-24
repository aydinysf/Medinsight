using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Doctors;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Admin;

public sealed record PendingVerificationDto(
    Guid VerificationId,
    Guid DoctorId,
    string DoctorFullName,
    string Specialty,
    string LicenseNumber,
    VerificationDocumentType DocumentType,
    string DocumentUrl,
    string? QrParsedData,
    DateTime SubmittedAtUtc);

public sealed class ListPendingVerificationsQueryHandler(IDoctorRepository doctors, IUserRepository users)
{
    public async Task<IReadOnlyList<PendingVerificationDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var pendingDoctors = await doctors.GetPendingVerificationAsync(cancellationToken);

        var result = new List<PendingVerificationDto>();
        foreach (var doctor in pendingDoctors)
        {
            var user = await users.GetByIdAsync(doctor.UserId, cancellationToken);
            result.AddRange(doctor.Verifications
                .Where(v => v.Status == VerificationStatus.Pending)
                .Select(v => new PendingVerificationDto(
                    v.Id, doctor.Id, user?.FullName ?? "?", doctor.Specialty, doctor.LicenseNumber,
                    v.DocumentType, v.DocumentUrl, v.QrParsedData, v.CreatedAtUtc)));
        }

        return result.OrderBy(v => v.SubmittedAtUtc).ToList();
    }
}

public sealed class ApproveVerificationHandler(IDoctorRepository doctors, ICurrentUser currentUser)
{
    /// <summary>Doğrulama kaydı bulunamazsa null döner (404).</summary>
    public async Task<VerificationDto?> HandleAsync(Guid verificationId, CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByVerificationIdAsync(verificationId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.ApproveVerification(verificationId, currentUser.UserId);
        await doctors.SaveChangesAsync(cancellationToken);

        return doctor.Verifications.First(v => v.Id == verificationId).ToDto();
    }
}

public sealed class RejectVerificationHandler(IDoctorRepository doctors, ICurrentUser currentUser)
{
    public async Task<VerificationDto?> HandleAsync(Guid verificationId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Ret gerekçesi zorunludur — doktor neden reddedildiğini görmelidir.");
        }

        var doctor = await doctors.GetByVerificationIdAsync(verificationId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.RejectVerification(verificationId, currentUser.UserId, reason);
        await doctors.SaveChangesAsync(cancellationToken);

        return doctor.Verifications.First(v => v.Id == verificationId).ToDto();
    }
}
