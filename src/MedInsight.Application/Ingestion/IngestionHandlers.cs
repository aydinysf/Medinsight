using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Quality;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Cases.Events;
using MedInsight.Domain.Common;

namespace MedInsight.Application.Ingestion;

/// <summary>Pipeline 1. aşama: DocumentUploaded → kural tabanlı sınıflandırma.</summary>
public sealed class OnDocumentUploadedClassify(ICaseRepository cases, IObjectStorage storage) : IDomainEventHandler<DocumentUploaded>
{
    public async Task HandleAsync(DocumentUploaded e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var document = medicalCase?.Documents.FirstOrDefault(d => d.Id == e.DocumentId);
        if (medicalCase is null || document is null || document.Status != DocumentStatus.Uploaded)
        {
            return; // idempotency: daha önce işlenmiş
        }

        var content = document.StorageKey is null
            ? []
            : await storage.DownloadAsync(document.StorageKey, cancellationToken);

        var type = DocumentClassifier.Classify(document.OriginalFileName, document.ContentType, content);
        if (type is null)
        {
            medicalCase.MarkDocumentClassificationFailed(e.DocumentId, "Dosya türü tanınamadı — desteklenen formatlar: DICOM, PDF, görüntü.");
        }
        else
        {
            medicalCase.ClassifyDocument(e.DocumentId, type.Value);
        }

        await cases.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Pipeline 3. aşama: DocumentClassified → Document Quality Engine.</summary>
public sealed class OnDocumentClassifiedRunQuality(ICaseRepository cases, IObjectStorage storage, QualityEngine qualityEngine) : IDomainEventHandler<DocumentClassified>
{
    public async Task HandleAsync(DocumentClassified e, CancellationToken cancellationToken)
    {
        var medicalCase = await cases.GetByIdAsync(e.CaseId!.Value, cancellationToken);
        var document = medicalCase?.Documents.FirstOrDefault(d => d.Id == e.DocumentId);
        if (medicalCase is null || document is null || document.Status != DocumentStatus.Classified)
        {
            return; // idempotency: daha önce işlenmiş
        }

        var content = document.StorageKey is null
            ? []
            : await storage.DownloadAsync(document.StorageKey, cancellationToken);

        var report = await qualityEngine.EvaluateAsync(new QualityContext(medicalCase, document, content), cancellationToken);
        medicalCase.ScoreDocumentQuality(e.DocumentId, report.OverallScore, report.CriteriaScores, report.FailureReasons, report.IsSufficient);

        await cases.SaveChangesAsync(cancellationToken);
    }
}
