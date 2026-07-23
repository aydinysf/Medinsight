using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public sealed class Measurement : Entity
{
    private Measurement()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid? StudyId { get; private set; }

    public Guid? SeriesId { get; private set; }

    public MeasurementType Type { get; private set; }

    public MeasurementMethod Method { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal Value { get; private set; }

    public string? Unit { get; private set; }

    public DateTime? MeasuredAtUtc { get; private set; }

    internal static Measurement Create(
        Guid caseId,
        string name,
        decimal value,
        MeasurementType type,
        MeasurementMethod method = MeasurementMethod.Manual,
        string? unit = null,
        DateTime? measuredAtUtc = null,
        Guid? studyId = null,
        Guid? seriesId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Measurement
        {
            CaseId = caseId,
            StudyId = studyId,
            SeriesId = seriesId,
            Type = type,
            Method = method,
            Name = name.Trim(),
            Value = value,
            Unit = unit,
            MeasuredAtUtc = measuredAtUtc is null ? null : DateTime.SpecifyKind(measuredAtUtc.Value, DateTimeKind.Utc),
        };
    }
}

public enum MeasurementType
{
    Diameter = 0,
    Area = 1,
    Volume = 2,
    Count = 3,
    Ratio = 4,
    Other = 5,
}

public enum MeasurementMethod
{
    Manual = 0,
    ImportedFromReport = 1,
    AiAssisted = 2,
    Other = 3,
}
