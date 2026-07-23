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

    public Modality Modality { get; private set; }

    public int SliceCount { get; private set; }

    internal static DicomSeries Create(Guid studyId, string? seriesInstanceUid, int? seriesNumber, string? description, Modality modality = Modality.Unknown)
    {
        return new DicomSeries
        {
            StudyId = studyId,
            SeriesInstanceUid = seriesInstanceUid,
            SeriesNumber = seriesNumber,
            Description = description,
            Modality = modality,
        };
    }

    internal void IncrementSliceCount() => SliceCount++;
}
