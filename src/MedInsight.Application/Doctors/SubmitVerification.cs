using System.Text.Json;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Doctors;

public sealed record SubmitVerification(
    VerificationDocumentType DocumentType,
    string FileName,
    string ContentType,
    byte[] DocumentContent,
    string? QrPayload);

public sealed record VerificationDto(
    Guid Id,
    Guid DoctorId,
    VerificationDocumentType DocumentType,
    VerificationMethod Method,
    VerificationStatus Status,
    string? QrParsedData,
    string? RejectionReason,
    DateTime CreatedAtUtc);

public sealed class SubmitVerificationHandler(IDoctorRepository doctors, IObjectStorage storage, ICurrentUser currentUser)
{
    public async Task<VerificationDto> HandleAsync(SubmitVerification command, CancellationToken cancellationToken = default)
    {
        var doctor = await doctors.GetByUserIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Doktor profili bulunamadı.");

        var storageKey = $"doctors/{doctor.Id}/verifications/{Guid.NewGuid():N}/{Path.GetFileName(command.FileName)}";
        using var stream = new MemoryStream(command.DocumentContent, writable: false);
        await storage.UploadAsync(storageKey, stream, command.ContentType, cancellationToken);

        // QR parse — MVP: ham içerik yapılandırılmış öneriye sarılır; gerçek QR görüntü
        // çözümleme (ZXing vb.) doğrulama pilotuyla birlikte gelecek. Admin'e ÖNERİ olarak gider.
        string? qrParsedData = string.IsNullOrWhiteSpace(command.QrPayload)
            ? null
            : JsonSerializer.Serialize(new { raw = command.QrPayload, parsedAtUtc = DateTime.UtcNow });

        var verification = doctor.SubmitVerification(command.DocumentType, storageKey, command.QrPayload, qrParsedData);
        await doctors.SaveChangesAsync(cancellationToken);

        return verification.ToDto();
    }
}

public static class VerificationMappings
{
    public static VerificationDto ToDto(this DoctorVerification verification) =>
        new(
            verification.Id,
            verification.DoctorId,
            verification.DocumentType,
            verification.Method,
            verification.Status,
            verification.QrParsedData,
            verification.RejectionReason,
            verification.CreatedAtUtc);
}
