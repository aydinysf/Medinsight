namespace MedInsight.Application.Abstractions.Dicom;

public sealed record DicomFileMetadata(
    string StudyInstanceUid,
    string SeriesInstanceUid,
    string? Modality,
    DateTime? StudyDate,
    int? SeriesNumber);

/// <summary>fo-dicom implementasyonu MedInsight.Dicom projesindedir.</summary>
public interface IDicomMetadataReader
{
    /// <summary>Geçerli DICOM değilse veya zorunlu UID'ler eksikse null döner.</summary>
    DicomFileMetadata? Read(byte[] content);
}
