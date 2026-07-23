using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases.Events;

public sealed record GroupedSeriesInfo
{
    public required Guid SeriesId { get; init; }

    public required Modality Modality { get; init; }

    public required int SliceCount { get; init; }
}

/// <summary>Bekleme penceresi kapandığında yayınlanır (bkz. ingestion-pipeline.md, 2. DICOM Grouping).</summary>
public sealed record DicomStudyGrouped : DomainEvent
{
    public required Guid StudyId { get; init; }

    public required List<GroupedSeriesInfo> SeriesList { get; init; }
}

public enum DocumentRoute
{
    TextExtraction = 0,
    RadiologyInference = 1,
    StorageOnly = 2,
}

/// <summary>Belgenin neden belirli bir işleme yoluna gittiğinin denetlenebilir kaydı.</summary>
public sealed record RoutingDecided : DomainEvent
{
    public required Guid DocumentId { get; init; }

    public required DocumentRoute Route { get; init; }
}
