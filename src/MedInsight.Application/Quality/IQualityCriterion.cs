using MedInsight.Domain.Cases;

namespace MedInsight.Application.Quality;

/// <summary>
/// Her kalite kriteri bağımsız bir plugin'dir — motor "çok büyüyecek"
/// (bkz. docs/domain/document-quality-engine.md, Büyüme Beklentisi).
/// </summary>
public interface IQualityCriterion
{
    string Name { get; }

    bool AppliesTo(DocumentType documentType);

    CriterionResult Evaluate(QualityContext context);
}

/// <summary>Skor 0–1; başarısızsa hastaya gösterilebilir somut bir neden taşır.</summary>
public sealed record CriterionResult(decimal Score, string? FailureReason = null);

public sealed record QualityContext(Case Case, MedicalDocument Document, byte[] Content);
