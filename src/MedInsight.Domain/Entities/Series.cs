using MedInsight.Domain.Common;

namespace MedInsight.Domain.Entities;

public sealed class Series : Entity
{
    private readonly List<Measurement> _measurements = [];

    private Series()
    {
    }

    public Guid StudyId { get; private set; }

    public int? SeriesNumber { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();

    public static Series Create(Guid studyId, int? seriesNumber = null, string? description = null)
    {
        return new Series
        {
            StudyId = studyId,
            SeriesNumber = seriesNumber,
            Description = description,
        };
    }
}
