using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public sealed class DicomSeries : Entity
{
    private DicomSeries()
    {
    }

    public Guid StudyId { get; private set; }

    public string? SeriesInstanceUid { get; private set; }

    public int? SeriesNumber { get; private set; }

    public string? Description { get; private set; }

    internal static DicomSeries Create(Guid studyId, string? seriesInstanceUid, int? seriesNumber, string? description)
    {
        return new DicomSeries
        {
            StudyId = studyId,
            SeriesInstanceUid = seriesInstanceUid,
            SeriesNumber = seriesNumber,
            Description = description,
        };
    }
}
