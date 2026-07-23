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

    /// <summary>Bekleme penceresi için: son DICOM dosyasının geldiği an (bkz. ingestion-pipeline.md).</summary>
    public DateTime LastFileReceivedAtUtc { get; private set; }

    public bool IsGrouped { get; private set; }

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
            LastFileReceivedAtUtc = DateTime.UtcNow,
        };
    }

    public DicomSeries AddSeries(string? seriesInstanceUid = null, int? seriesNumber = null, string? description = null, Modality modality = Modality.Unknown)
    {
        var series = DicomSeries.Create(Id, seriesInstanceUid, seriesNumber, description, modality);
        _series.Add(series);
        return series;
    }

    /// <summary>Toplu yüklemede gelen her DICOM dosyası: seriyi bul/oluştur, slice sayısını artır, pencereyi yenile.</summary>
    internal DicomSeries RegisterFile(string seriesInstanceUid, Modality modality, int? seriesNumber)
    {
        var series = _series.FirstOrDefault(s => s.SeriesInstanceUid == seriesInstanceUid);
        if (series is null)
        {
            series = DicomSeries.Create(Id, seriesInstanceUid, seriesNumber, null, modality);
            _series.Add(series);
        }

        series.IncrementSliceCount();
        LastFileReceivedAtUtc = DateTime.UtcNow;
        IsGrouped = false;
        return series;
    }

    internal void MarkGrouped()
    {
        if (IsGrouped)
        {
            throw new DomainException("Çalışma zaten gruplanmış.");
        }

        IsGrouped = true;
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
