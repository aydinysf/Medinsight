using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace MedInsight.AIOrchestration.Pipeline;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Altında kalan analizler doktor önceliğini yükseltir (ADR-004). MVP: tek global değer.</summary>
    public decimal ConfidenceThreshold { get; set; } = 0.6m;
}

/// <summary>
/// Üç kapı (bkz. ai/guardrails-and-boundaries.md): confidence eşiği, kapsam
/// kontrolü, kaynak izlenebilirliği. Reasoning ne üretirse üretsin bu katman
/// tanı/doz/tedavi ifadelerini teknik olarak engeller — nezaket değil, kural.
/// </summary>
public sealed partial class Guardrails(IOptions<AiOptions> options)
{
    public const string ScopeRedirect =
        "Bu değerlendirme yalnızca bilgilendirme amaçlıdır; tanı ve tedavi kararları için doktorunuzla görüşmeniz gerekir.";

    // Yalnızca İDDİA kipleri yakalanır ("tanı koyuyorum", "tanınız: ...");
    // olumsuzlama/açıklama ("ben tanı koymam", "tanı koyması doktorunundur")
    // serbesttir — aksi halde Hızır'ın sınırlarını anlattığı her yanıt siliniyordu.
    [GeneratedRegex(@"(kesin\s+tan[ıi]|tan[ıi]n[ıi]z\s*:|te[şs]hisiniz\s*:|te[şs]his\s+koy(uyorum|dum|d[uü]k|ar[ıi]m|acağ[ıi]m|abilirim)|tan[ıi]\s+koy(uyorum|dum|d[uü]k|ar[ıi]m|acağ[ıi]m|abilirim)|hastal[ıi][ğg][ıi]n[ıi]z\s+kesin|almal[ıi]s[ıi]n[ıi]z\s+\d+\s*mg|\d+\s*mg\s+al|doz[uü]n[uü]z|tedavi\s+olarak\s+.+\s+kullan)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ForbiddenScopeRegex();

    /// <summary>Kapı 1 — confidence eşiği.</summary>
    public bool IsLowConfidence(decimal confidenceScore) => confidenceScore < options.Value.ConfidenceThreshold;

    /// <summary>Kapı 2 — kapsam kontrolü: yasaklı ifade içeren metin zorunlu yönlendirmeyle değiştirilir.</summary>
    public string EnforceScope(string text) =>
        ForbiddenScopeRegex().IsMatch(text) ? ScopeRedirect : text;

    public bool ViolatesScope(string text) => ForbiddenScopeRegex().IsMatch(text);

    /// <summary>Kapı 3 — kaynak izlenebilirliği: belgeye dayanmayan bulgu elenir.</summary>
    public IReadOnlyList<LlmFinding> EnforceSourceTraceability(IReadOnlyList<LlmFinding> findings) =>
        findings.Where(f => f.SourceDocumentId is not null).ToList();
}
