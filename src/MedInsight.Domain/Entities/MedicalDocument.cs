using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class MedicalDocument : Entity
{
    private MedicalDocument()
    {
    }

    public Guid MedicalCaseId { get; private set; }

    public Guid? StudyId { get; private set; }

    public DocumentType Type { get; private set; }

    public string Title { get; private set; } = null!;

    public DateTime? DocumentDateUtc { get; private set; }

    public string? ContentText { get; private set; }

    public static MedicalDocument Create(
        Guid medicalCaseId,
        string title,
        DocumentType type = DocumentType.Unknown,
        DateTime? documentDateUtc = null,
        Guid? studyId = null,
        string? contentText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new MedicalDocument
        {
            MedicalCaseId = medicalCaseId,
            StudyId = studyId,
            Type = type,
            Title = title.Trim(),
            DocumentDateUtc = documentDateUtc is null ? null : DateTime.SpecifyKind(documentDateUtc.Value, DateTimeKind.Utc),
            ContentText = contentText,
        };
    }
}
