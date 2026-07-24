using MedInsight.Domain.Common;

namespace MedInsight.Domain.Identity;

public enum VerificationDocumentType
{
    Diploma = 0,
    SpecialtyCertificate = 1,
    TTBRegistry = 2,
}

public enum VerificationMethod
{
    Manual = 0,
    QrParsed = 1,
    ExternalApi = 2,
}

/// <summary>
/// Yarı otomatik doğrulama kaydı (ADR-007): QR parse sonucu admin'e yalnızca
/// ÖNERİ olarak sunulur — hiçbir adım otomatik onaya izin vermez.
/// </summary>
public sealed class DoctorVerification : Entity
{
    private DoctorVerification()
    {
    }

    public Guid DoctorId { get; private set; }

    public VerificationDocumentType DocumentType { get; private set; }

    public string DocumentUrl { get; private set; } = null!;

    public string? QrPayload { get; private set; }

    /// <summary>Parse edilmiş QR verisi (jsonb) — admin'e öneri.</summary>
    public string? QrParsedData { get; private set; }

    public VerificationMethod Method { get; private set; }

    public VerificationStatus Status { get; private set; }

    public Guid? VerifiedByAdminId { get; private set; }

    public DateTime? VerifiedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    internal static DoctorVerification Create(Guid doctorId, VerificationDocumentType documentType, string documentUrl, string? qrPayload, string? qrParsedData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentUrl);

        return new DoctorVerification
        {
            DoctorId = doctorId,
            DocumentType = documentType,
            DocumentUrl = documentUrl,
            QrPayload = qrPayload,
            QrParsedData = qrParsedData,
            Method = string.IsNullOrWhiteSpace(qrPayload) ? VerificationMethod.Manual : VerificationMethod.QrParsed,
            Status = VerificationStatus.Pending,
        };
    }

    internal void Approve(Guid adminId)
    {
        EnsurePending();
        Status = VerificationStatus.Verified;
        VerifiedByAdminId = adminId;
        VerifiedAtUtc = DateTime.UtcNow;
    }

    internal void Reject(Guid adminId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsurePending();
        Status = VerificationStatus.Rejected;
        VerifiedByAdminId = adminId;
        VerifiedAtUtc = DateTime.UtcNow;
        RejectionReason = reason;
    }

    private void EnsurePending()
    {
        if (Status != VerificationStatus.Pending)
        {
            throw new DomainException("Bu doğrulama kaydı zaten sonuçlandırılmış.");
        }
    }
}
