namespace MedInsight.Application.Abstractions.Ai;

/// <summary>
/// Contract for the future Python AI analysis service.
/// MedInsight is a Clinical Decision Support System (CDSS): implementations
/// must only organize, compare and analyze medical records to support
/// physician decision-making. They must never produce a diagnosis.
/// </summary>
public interface IAiService
{
    Task<AiAnalysisResult> AnalyzeAsync(AiAnalysisRequest request, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

public sealed record AiAnalysisRequest(string CorrelationId, string AnalysisType, IReadOnlyDictionary<string, string> Parameters);

public sealed record AiAnalysisResult(string CorrelationId, string Summary, IReadOnlyDictionary<string, string> Insights);
