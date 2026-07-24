namespace MedInsight.AIOrchestration;

/// <summary>
/// Prompt-injection savunması yapısal olarak burada başlar: belge içeriği yalnızca
/// ClinicalContext alanına girer, SystemInstructions'a asla karışmaz
/// (bkz. docs/ai/guardrails-and-boundaries.md).
/// </summary>
public sealed record LlmRequest(string SystemInstructions, string ClinicalContext, string UserMessage);

public sealed record LlmFinding(string Description, Guid? SourceDocumentId);

public sealed record LlmDifferential(string Name, decimal ConfidenceScore, string RiskLevel, IReadOnlyList<int> SourceFindingIndexes);

public sealed record LlmResult(
    string Summary,
    IReadOnlyList<LlmFinding> Findings,
    IReadOnlyList<LlmDifferential> Differentials,
    decimal ConfidenceScore,
    string ModelVersion,
    string PromptVersion);

/// <summary>AI sağlayıcı soyutlaması — sağlayıcı değişimi Domain'e dokunmaz (layered-architecture.md).</summary>
public interface ILlmClient
{
    Task<LlmResult> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
