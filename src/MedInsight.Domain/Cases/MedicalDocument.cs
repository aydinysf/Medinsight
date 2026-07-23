using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public sealed class MedicalDocument : Entity
{
    private MedicalDocument()
    {
    }

    public Guid CaseId { get; private set; }

    public string Title { get; private set; } = null!;

    public DocumentType Type { get; private set; }

    public DocumentStatus Status { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public string? StorageKey { get; private set; }

    public string? OriginalFileName { get; private set; }

    public string? ContentType { get; private set; }

    public long SizeBytes { get; private set; }

    /// <summary>SHA-256 hex — Duplicated Files kalite kriteri için (bkz. document-quality-engine.md).</summary>
    public string? ContentHash { get; private set; }

    internal static MedicalDocument Create(
        Guid caseId,
        string title,
        Guid uploadedByUserId,
        string? storageKey = null,
        string? originalFileName = null,
        string? contentType = null,
        long sizeBytes = 0,
        string? contentHash = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new MedicalDocument
        {
            CaseId = caseId,
            Title = title.Trim(),
            Type = DocumentType.Unknown,
            Status = DocumentStatus.Uploaded,
            UploadedByUserId = uploadedByUserId,
            StorageKey = storageKey,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            ContentHash = contentHash,
        };
    }

    internal void Classify(DocumentType type)
    {
        Type = type;
        Status = DocumentStatus.Classified;
    }

    internal void MarkClassificationFailed() => Status = DocumentStatus.ClassificationFailed;

    internal void MarkQualityChecked() => Status = DocumentStatus.QualityChecked;

    internal void Reject() => Status = DocumentStatus.Rejected;
}

/// <summary>Sınıflandırma tipleri — bkz. docs/architecture/ingestion-pipeline.md</summary>
public enum DocumentType
{
    Unknown = 0,
    DicomFile = 1,
    TextualReport = 2,
    ScannedReport = 3,
    PhotoDocument = 4,
}

public enum DocumentStatus
{
    Uploaded = 0,
    Classified = 1,
    QualityChecked = 2,
    Rejected = 3,
    ClassificationFailed = 4,
}
