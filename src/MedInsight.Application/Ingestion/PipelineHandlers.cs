using MedInsight.Application.Abstractions.Dicom;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Abstractions.TextExtraction;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Application.Ingestion;

/// <summary>Pipeline 2. aşama: DicomFile sınıflandırıldı → study/series gruplama kaydı.</summary>
public sealed class OnDocumentClassifiedGroupDicom(ICaseRepository cases, IObjectStorage storage, IDicomMetadataReader dicomReader)
    : IDomainEventHandler<DocumentClassified>
{
    public async Task HandleAsync(DocumentClassified e, CancellationToken cancellationToken)
    {
        if (e.DocumentType != DocumentType.DicomFile)
        {
            return;
        }

        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var document = medicalCase?.Documents.FirstOrDefault(d => d.Id == e.DocumentId);
        if (medicalCase is null || document?.StorageKey is null)
        {
            return;
        }

        var metadata = dicomReader.Read(await storage.DownloadAsync(document.StorageKey, cancellationToken));
        if (metadata is null)
        {
            return; // DicomIntegrity kriteri kalite aşamasında zaten düşük skor verir
        }

        var modality = Enum.TryParse<Modality>(metadata.Modality, ignoreCase: true, out var parsed) ? parsed : Modality.Other;
        medicalCase.RegisterDicomFile(metadata.StudyInstanceUid, metadata.SeriesInstanceUid, modality, metadata.StudyDate, metadata.SeriesNumber);

        await cases.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Pipeline 4. aşama: kaliteden geçen belge için yönlendirme kararı (RoutingDecided).</summary>
public sealed class OnDocumentQualityScoredRoute(ICaseRepository cases) : IDomainEventHandler<DocumentQualityScored>
{
    public async Task HandleAsync(DocumentQualityScored e, CancellationToken cancellationToken)
    {
        if (!e.IsSufficient)
        {
            return;
        }

        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var document = medicalCase?.Documents.FirstOrDefault(d => d.Id == e.DocumentId);
        if (medicalCase is null || document is null || document.Status != DocumentStatus.QualityChecked)
        {
            return;
        }

        medicalCase.DecideRouting(e.DocumentId);
        await cases.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Text Extraction Service: TextualReport → PDF metin katmanı; ScannedReport → OCR (ADR-011).</summary>
public sealed class OnRoutingDecidedExtractText(
    ICaseRepository cases,
    IObjectStorage storage,
    IPdfTextExtractor pdfTextExtractor,
    IOcrProvider ocrProvider) : IDomainEventHandler<RoutingDecided>
{
    public async Task HandleAsync(RoutingDecided e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var document = medicalCase?.Documents.FirstOrDefault(d => d.Id == e.DocumentId);
        if (medicalCase is null || document is null)
        {
            return;
        }

        if (e.Route == DocumentRoute.TextExtraction && document.StorageKey is not null && document.ExtractedText is null)
        {
            var content = await storage.DownloadAsync(document.StorageKey, cancellationToken);

            string? text;
            decimal? confidence = null;
            if (document.Type == DocumentType.TextualReport)
            {
                text = pdfTextExtractor.ExtractText(content);
            }
            else
            {
                var ocr = await ocrProvider.ExtractTextAsync(content, cancellationToken);
                text = ocr.Text;
                confidence = ocr.ConfidenceScore;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                medicalCase.StoreExtractedText(e.DocumentId, text, confidence);
            }
        }

        // Pipeline sırası: routing (ve varsa metin çıkarma) tamamlandı → AI analizi
        // talep edilebilir. Outbox sıralı işlediği için aynı batch'teki diğer
        // belgelerin RoutingDecided'ları bu talepten önce işlenir.
        medicalCase.RequestAiAnalysis();
        await cases.SaveChangesAsync(cancellationToken);
    }
}
