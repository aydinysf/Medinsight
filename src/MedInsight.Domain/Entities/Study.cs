using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class Study : Entity
{
    private readonly List<Series> _series = [];
    private readonly List<MedicalDocument> _documents = [];
    private readonly List<Measurement> _measurements = [];

    private Study()
    {
    }

    public Guid MedicalCaseId { get; private set; }

    public Modality Modality { get; private set; }

    public DateTime StudyDateUtc { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<Series> Series => _series.AsReadOnly();

    public IReadOnlyCollection<MedicalDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();

    public static Study Create(Guid medicalCaseId, Modality modality, DateTime studyDateUtc, string? description = null)
    {
        return new Study
        {
            MedicalCaseId = medicalCaseId,
            Modality = modality,
            StudyDateUtc = DateTime.SpecifyKind(studyDateUtc, DateTimeKind.Utc),
            Description = description,
        };
    }
}
