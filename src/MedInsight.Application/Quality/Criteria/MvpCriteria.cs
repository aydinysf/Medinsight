using MedInsight.Application.Abstractions.Dicom;
using MedInsight.Application.Abstractions.TextExtraction;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Quality.Criteria;

/// <summary>Hash bazlı tekrar tespiti — aynı içerik aynı vakaya ikinci kez yüklenmiş mi.</summary>
public sealed class DuplicatedFilesCriterion : IQualityCriterion
{
    public string Name => "DuplicatedFiles";

    public bool AppliesTo(DocumentType documentType) => true;

    public Task<CriterionResult> EvaluateAsync(QualityContext context, CancellationToken cancellationToken = default)
    {
        var isDuplicate = context.Document.ContentHash is not null
            && context.Case.Documents.Any(d =>
                d.Id != context.Document.Id
                && d.ContentHash == context.Document.ContentHash
                && d.Status != DocumentStatus.Rejected);

        return Task.FromResult(isDuplicate
            ? new CriterionResult(0, "Bu dosya vakaya daha önce yüklenmiş görünüyor (aynı içerik).")
            : new CriterionResult(1));
    }
}

/// <summary>Dosyanın boş/bozuk olmadığının temel kontrolü.</summary>
public sealed class CompletenessCriterion : IQualityCriterion
{
    public string Name => "Completeness";

    public bool AppliesTo(DocumentType documentType) => true;

    public Task<CriterionResult> EvaluateAsync(QualityContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(context.Content.Length == 0 || context.Document.SizeBytes == 0
            ? new CriterionResult(0, "Dosya boş görünüyor.")
            : new CriterionResult(1));
}

/// <summary>
/// DICOM bütünlüğü: zorunlu metadata alanlarının varlığı — PatientID, StudyDate,
/// Modality (bkz. document-quality-engine.md).
/// </summary>
public sealed class DicomIntegrityCriterion(IDicomMetadataReader dicomReader) : IQualityCriterion
{
    public string Name => "DicomIntegrity";

    public bool AppliesTo(DocumentType documentType) => documentType == DocumentType.DicomFile;

    public Task<CriterionResult> EvaluateAsync(QualityContext context, CancellationToken cancellationToken = default)
    {
        var integrity = dicomReader.ReadIntegrity(context.Content);
        if (integrity is null)
        {
            return Task.FromResult(new CriterionResult(0, "DICOM dosya yapısı doğrulanamadı."));
        }

        var missing = new List<string>();
        if (!integrity.HasPatientId)
        {
            missing.Add("PatientID");
        }

        if (!integrity.HasStudyDate)
        {
            missing.Add("StudyDate");
        }

        if (!integrity.HasModality)
        {
            missing.Add("Modality");
        }

        if (missing.Count == 0)
        {
            return Task.FromResult(new CriterionResult(1));
        }

        var score = Math.Round(1 - (missing.Count / 3m), 4);
        return Task.FromResult(new CriterionResult(score, $"Zorunlu DICOM alanları eksik: {string.Join(", ", missing)}."));
    }
}

/// <summary>
/// OCR motorunun kendi güven skoru (taranan PDF / fotoğraf). Stub sağlayıcı
/// aktifken uygulanmaz — skor üretemeyen sağlayıcı belgeyi cezalandırmamalı.
/// </summary>
public sealed class OcrScoreCriterion(IOcrProvider ocrProvider) : IQualityCriterion
{
    public string Name => "OcrScore";

    public bool AppliesTo(DocumentType documentType) =>
        ocrProvider.Name != "Stub"
        && documentType is DocumentType.ScannedReport or DocumentType.PhotoDocument;

    public async Task<CriterionResult> EvaluateAsync(QualityContext context, CancellationToken cancellationToken = default)
    {
        var result = await ocrProvider.ExtractTextAsync(context.Content, cancellationToken);
        return result.ConfidenceScore < 0.5m
            ? new CriterionResult(result.ConfidenceScore, "Belge OCR ile güvenilir şekilde okunamadı — daha net bir tarama/fotoğraf yükleyin.")
            : new CriterionResult(result.ConfidenceScore);
    }
}
