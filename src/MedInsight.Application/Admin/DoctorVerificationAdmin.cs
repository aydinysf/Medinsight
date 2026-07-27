using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
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

public sealed record VerificationDocumentContent(byte[] Content, string FileName, string ContentType);

/// <summary>
/// Doğrulama belgesi API üzerinden stream edilir (JWT korumalı) — presigned URL
/// tarayıcı için kullanılmaz; dev'de PresignEndpoint yalnızca konteyner ağına açıktır.
/// </summary>
public sealed class GetVerificationDocumentQueryHandler(IDoctorRepository doctors, IObjectStorage storage)
{
    public async Task<VerificationDocumentContent?> HandleAsync(Guid verificationId, CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByVerificationIdAsync(verificationId, cancellationToken);
        var verification = doctor?.Verifications.FirstOrDefault(v => v.Id == verificationId);
        if (verification is null)
        {
            return null;
        }

        var fileName = Path.GetFileName(verification.DocumentUrl);
        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream",
        };

        var content = await storage.DownloadAsync(verification.DocumentUrl, cancellationToken);
        return new VerificationDocumentContent(content, fileName, contentType);
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
