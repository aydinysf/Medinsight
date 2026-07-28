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

/// <summary>Sohbet geçmişi dönüşü: Role = "user" | "assistant".</summary>
public sealed record LlmChatTurn(string Role, string Content);

/// <summary>
/// Hızır sohbeti (ADR-018). Analizin JSON sözleşmesinden bağımsız serbest metin;
/// prompt-injection savunması aynı: bağlam ve kullanıcı mesajları sistem
/// talimatına asla karışmaz.
/// </summary>
public sealed record LlmChatRequest(
    string SystemInstructions,
    string ClinicalContext,
    IReadOnlyList<LlmChatTurn> History,
    string UserMessage);

/// <summary>AI sağlayıcı soyutlaması — sağlayıcı değişimi Domain'e dokunmaz (layered-architecture.md).</summary>
public interface ILlmClient
{
    Task<LlmResult> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>Serbest metin sohbet yanıtı (ADR-018) — çıktı Guardrails.EnforceScope'tan geçirilmelidir.</summary>
    Task<string> ChatAsync(LlmChatRequest request, CancellationToken cancellationToken = default);
}
