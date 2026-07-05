using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class MedicalCase : Entity
{
    private readonly List<Study> _studies = [];
    private readonly List<MedicalDocument> _documents = [];
    private readonly List<Measurement> _measurements = [];
    private readonly List<TimelineEvent> _timelineEvents = [];

    private MedicalCase()
    {
    }

    public Guid PatientId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public BodySystem BodySystem { get; private set; }

    public MedicalCaseStatus Status { get; private set; }

    public IReadOnlyCollection<Study> Studies => _studies.AsReadOnly();

    public IReadOnlyCollection<MedicalDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();

    public IReadOnlyCollection<TimelineEvent> TimelineEvents => _timelineEvents.AsReadOnly();

    public static MedicalCase Create(Guid patientId, string title, BodySystem bodySystem = BodySystem.Unknown, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new MedicalCase
        {
            PatientId = patientId,
            Title = title.Trim(),
            Description = description,
            BodySystem = bodySystem,
            Status = MedicalCaseStatus.Active,
        };
    }

    public void Close() => Status = MedicalCaseStatus.Closed;

    public void Archive() => Status = MedicalCaseStatus.Archived;

    public void Reopen() => Status = MedicalCaseStatus.Active;
}
