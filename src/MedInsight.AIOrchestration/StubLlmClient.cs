namespace MedInsight.AIOrchestration;

/// <summary>
/// Gerçek LLM sağlayıcısı bağlanana kadar deterministik yer tutucu.
/// Tanı adayı ÜRETMEZ (guardrails: emin olmadığın yerde sus); yalnızca eldeki
/// belge metinlerinden yorumsuz bulgu ve özet çıkarır. Metin yoksa güven düşüktür.
/// Gerçek sağlayıcıya geçiş için bkz. DependencyInjection.cs — TODO(llm-provider).
/// </summary>
public sealed class StubLlmClient : ILlmClient
{
    public const string Model = "hizir-stub-0.1";
    public const string Prompt = "hizir-prompt-v1";

    public Task<LlmResult> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var documentSections = request.ClinicalContext
            .Replace("\r\n", "\n")
            .Split("\n---\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.StartsWith("[BELGE:", StringComparison.Ordinal))
            .ToList();

        var findings = new List<LlmFinding>();
        foreach (var section in documentSections)
        {
            var headerEnd = section.IndexOf(']');
            if (headerEnd < 0)
            {
                continue;
            }

            var idPart = section[7..headerEnd];
            var text = section[(headerEnd + 1)..].Trim();
            if (text.Length == 0 || !Guid.TryParse(idPart, out var documentId))
            {
                continue;
            }

            var snippet = text.Length > 200 ? text[..200] + "…" : text;
            findings.Add(new LlmFinding($"Belge metninde geçen ifade: \"{snippet}\"", documentId));
        }

        var confidence = findings.Count > 0 ? 0.72m : 0.35m;
        var summary = findings.Count > 0
            ? $"Vakada metni okunabilen {findings.Count} belge incelendi. Belge içerikleri yorum yapılmadan bulgu olarak listelendi; klinik değerlendirme doktor incelemesindedir."
            : "Vakadaki belgelerden analiz edilebilir metin çıkarılamadı; değerlendirme için doktor incelemesi gereklidir.";

        return Task.FromResult(new LlmResult(summary, findings, [], confidence, Model, Prompt));
    }
}
