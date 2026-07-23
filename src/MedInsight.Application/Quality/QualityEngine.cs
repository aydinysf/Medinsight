using Microsoft.Extensions.Options;

namespace MedInsight.Application.Quality;

public sealed class QualityOptions
{
    public const string SectionName = "Quality";

    /// <summary>Bu skorun altındaki belgeler analiz kuyruğuna giremez.</summary>
    public decimal SufficientScore { get; set; } = 0.7m;
}

public sealed record QualityReport(
    decimal OverallScore,
    IReadOnlyDictionary<string, decimal> CriteriaScores,
    IReadOnlyList<string> FailureReasons,
    bool IsSufficient);

public sealed class QualityEngine(IEnumerable<IQualityCriterion> criteria, IOptions<QualityOptions> options)
{
    public QualityReport Evaluate(QualityContext context)
    {
        var applicable = criteria.Where(c => c.AppliesTo(context.Document.Type)).ToList();
        if (applicable.Count == 0)
        {
            return new QualityReport(0, new Dictionary<string, decimal>(), ["Bu belge türü için kalite kriteri tanımlı değil."], false);
        }

        var scores = new Dictionary<string, decimal>();
        var failures = new List<string>();

        foreach (var criterion in applicable)
        {
            var result = criterion.Evaluate(context);
            scores[criterion.Name] = result.Score;
            if (result.FailureReason is not null)
            {
                failures.Add($"{criterion.Name}: {result.FailureReason}");
            }
        }

        var overall = Math.Round(scores.Values.Average(), 4);
        return new QualityReport(overall, scores, failures, overall >= options.Value.SufficientScore);
    }
}
