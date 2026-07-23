using FellowOakDicom;
using MedInsight.Application.Abstractions.Dicom;

namespace MedInsight.Dicom;

public sealed class FoDicomMetadataReader : IDicomMetadataReader
{
    public DicomFileMetadata? Read(byte[] content)
    {
        try
        {
            using var stream = new MemoryStream(content, writable: false);
            var file = DicomFile.Open(stream, FileReadOption.ReadAll);
            var dataset = file.Dataset;

            var studyUid = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
            var seriesUid = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            if (string.IsNullOrWhiteSpace(studyUid) || string.IsNullOrWhiteSpace(seriesUid))
            {
                return null;
            }

            var modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);
            DateTime? studyDate = dataset.TryGetSingleValue<DateTime>(DicomTag.StudyDate, out var date)
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                : null;
            int? seriesNumber = dataset.TryGetSingleValue<int>(DicomTag.SeriesNumber, out var number) ? number : null;

            return new DicomFileMetadata(studyUid, seriesUid, string.IsNullOrWhiteSpace(modality) ? null : modality, studyDate, seriesNumber);
        }
        catch (DicomException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
