using MedInsight.Domain.Common;
using MedInsight.Domain.Enums;

namespace MedInsight.Domain.Entities;

public sealed class Measurement : Entity
{
    private Measurement()
    {
    }

    public Guid MedicalCaseId { get; private set; }

    public Guid? StudyId { get; private set; }

    public Guid? SeriesId { get; private set; }

    public MeasurementType Type { get; private set; }

    public MeasurementMethod Method { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal Value { get; private set; }

    public string? Unit { get; private set; }

    public DateTime? MeasuredAtUtc { get; private set; }

    public static Measurement Create(
        Guid medicalCaseId,
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
            MedicalCaseId = medicalCaseId,
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
