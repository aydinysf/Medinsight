using MedInsight.Domain.Cases;

namespace MedInsight.Application.Quality.Criteria;

/// <summary>Hash bazlı tekrar tespiti — aynı içerik aynı vakaya ikinci kez yüklenmiş mi.</summary>
public sealed class DuplicatedFilesCriterion : IQualityCriterion
{
    public string Name => "DuplicatedFiles";

    public bool AppliesTo(DocumentType documentType) => true;

    public CriterionResult Evaluate(QualityContext context)
    {
        var isDuplicate = context.Document.ContentHash is not null
            && context.Case.Documents.Any(d =>
                d.Id != context.Document.Id
                && d.ContentHash == context.Document.ContentHash
                && d.Status != DocumentStatus.Rejected);

        return isDuplicate
            ? new CriterionResult(0, "Bu dosya vakaya daha önce yüklenmiş görünüyor (aynı içerik).")
            : new CriterionResult(1);
    }
}

/// <summary>Dosyanın boş/bozuk olmadığının temel kontrolü.</summary>
public sealed class CompletenessCriterion : IQualityCriterion
{
    public string Name => "Completeness";

    public bool AppliesTo(DocumentType documentType) => true;

    public CriterionResult Evaluate(QualityContext context) =>
        context.Content.Length == 0 || context.Document.SizeBytes == 0
            ? new CriterionResult(0, "Dosya boş görünüyor.")
            : new CriterionResult(1);
}

/// <summary>
/// DICOM bütünlüğü — MVP dilim 1: preamble kontrolü. Zorunlu metadata alanları
/// (PatientID, StudyDate, Modality) DICOM gruplama dilimiyle birlikte gelecek.
/// </summary>
public sealed class DicomIntegrityCriterion : IQualityCriterion
{
    public string Name => "DicomIntegrity";

    public bool AppliesTo(DocumentType documentType) => documentType == DocumentType.DicomFile;

    public CriterionResult Evaluate(QualityContext context)
    {
        var content = context.Content;
        var hasPreamble = content.Length >= 132
            && content[128] == (byte)'D' && content[129] == (byte)'I'
            && content[130] == (byte)'C' && content[131] == (byte)'M';

        return hasPreamble
            ? new CriterionResult(1)
            : new CriterionResult(0, "DICOM dosya yapısı doğrulanamadı (DICM başlığı eksik).");
    }
}
