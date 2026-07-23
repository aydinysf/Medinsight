using Microsoft.Extensions.Options;

namespace MedInsight.Application.Quality;

public sealed class QualityOptions
{
    public const string SectionName = "Quality";

    /// <summary>Bu skorun altındaki belgeler analiz kuyruğuna giremez.</summary>
    public decimal SufficientScore { get; set; } = 0.7m;

    /// <summary>
    /// Kriter ağırlıkları — kod değişikliği gerektirmeden güncellenebilir
    /// (bkz. document-quality-engine.md, Skor Ağırlıklandırma). Tanımsız kriter = 1.
    /// </summary>
    public Dictionary<string, decimal> Weights { get; set; } = [];
}

public sealed record QualityReport(
    decimal OverallScore,
    IReadOnlyDictionary<string, decimal> CriteriaScores,
    IReadOnlyList<string> FailureReasons,
    bool IsSufficient);

public sealed class QualityEngine(IEnumerable<IQualityCriterion> criteria, IOptions<QualityOptions> options)
{
    public async Task<QualityReport> EvaluateAsync(QualityContext context, CancellationToken cancellationToken = default)
    {
        var applicable = criteria.Where(c => c.AppliesTo(context.Document.Type)).ToList();
        if (applicable.Count == 0)
        {
            return new QualityReport(0, new Dictionary<string, decimal>(), ["Bu belge türü için kalite kriteri tanımlı değil."], false);
        }

        var scores = new Dictionary<string, decimal>();
        var failures = new List<string>();
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (var criterion in applicable)
        {
            var result = await criterion.EvaluateAsync(context, cancellationToken);
            var weight = options.Value.Weights.GetValueOrDefault(criterion.Name, 1m);

            scores[criterion.Name] = result.Score;
            weightedSum += result.Score * weight;
            totalWeight += weight;

            if (result.FailureReason is not null)
            {
                failures.Add($"{criterion.Name}: {result.FailureReason}");
            }
        }

        var overall = totalWeight == 0 ? 0 : Math.Round(weightedSum / totalWeight, 4);
        return new QualityReport(overall, scores, failures, overall >= options.Value.SufficientScore);
    }
}
