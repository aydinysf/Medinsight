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

    internal static MedicalDocument Create(Guid caseId, string title, Guid uploadedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new MedicalDocument
        {
            CaseId = caseId,
            Title = title.Trim(),
            Type = DocumentType.Unknown,
            Status = DocumentStatus.Uploaded,
            UploadedByUserId = uploadedByUserId,
        };
    }

    public void Classify(DocumentType type)
    {
        Type = type;
        Status = DocumentStatus.Classified;
    }

    public void MarkQualityChecked() => Status = DocumentStatus.QualityChecked;

    public void Reject() => Status = DocumentStatus.Rejected;
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
}
