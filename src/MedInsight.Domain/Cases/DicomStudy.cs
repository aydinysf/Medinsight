using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public sealed class DicomStudy : Entity
{
    private readonly List<DicomSeries> _series = [];

    private DicomStudy()
    {
    }

    public Guid CaseId { get; private set; }

    public string? StudyInstanceUid { get; private set; }

    public Modality Modality { get; private set; }

    public DateTime StudyDateUtc { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<DicomSeries> Series => _series.AsReadOnly();

    internal static DicomStudy Create(Guid caseId, Modality modality, DateTime studyDateUtc, string? studyInstanceUid = null, string? description = null)
    {
        return new DicomStudy
        {
            CaseId = caseId,
            StudyInstanceUid = studyInstanceUid,
            Modality = modality,
            StudyDateUtc = DateTime.SpecifyKind(studyDateUtc, DateTimeKind.Utc),
            Description = description,
        };
    }

    public DicomSeries AddSeries(string? seriesInstanceUid = null, int? seriesNumber = null, string? description = null)
    {
        var series = DicomSeries.Create(Id, seriesInstanceUid, seriesNumber, description);
        _series.Add(series);
        return series;
    }
}

public enum Modality
{
    Unknown = 0,
    MR = 1,
    CT = 2,
    PET = 3,
    EEG = 4,
    US = 5,
    Other = 6,
}
