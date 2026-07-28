using System.Text.Json;

namespace MedInsight.AIOrchestration;

/// <summary>
/// Sağlayıcıdan bağımsız JSON çıktı sözleşmesi ve savunmacı ayrıştırıcı.
/// Model şemadan saparsa bulgu üretilmez, güven eşiğin altına düşer →
/// ADR-004 gereği doktor önceliği yükselir; geçersiz kaynak guid'i null olur
/// ve kaynak izlenebilirliği kapısında elenir.
/// </summary>
public static class LlmJsonContract
{
    public const string OutputContract =
        "YANIT BİÇİMİ: Yalnızca geçerli JSON döndür, başka hiçbir metin yazma. Şema: " +
        "{\"summary\": string (doktora yönelik yorumsuz özet, Türkçe), " +
        "\"confidence\": number (0-1 arası; kanıt zayıfsa düşük ver), " +
        "\"findings\": [{\"description\": string, \"sourceDocumentId\": string}], " +
        "\"differentials\": [{\"name\": string, \"confidence\": number, \"riskLevel\": \"Low\"|\"Medium\"|\"High\", \"sourceFindingIndexes\": [int]}]}. " +
        "KURALLAR: Her bulgunun sourceDocumentId'si, bağlamdaki [BELGE:<guid>] başlığındaki guid olmalıdır; " +
        "belgeye dayandıramadığın bulguyu HİÇ yazma. differentials yalnızca olasılık sıralamasıdır, kesin tanı değildir; " +
        "sourceFindingIndexes findings dizisindeki 0 tabanlı indekslerdir. Emin değilsen differentials boş bırak.";

    public static LlmResult ParseResult(string text, string model, string promptVersion)
    {
        var json = StripCodeFences(text);

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            // Şemaya uymayan yanıt: bulgu üretme, düşük güvenle doktora bırak.
            return new LlmResult(
                "Model yanıtı beklenen biçimde değildi; değerlendirme için doktor incelemesi gereklidir.",
                [], [], 0.2m, model, promptVersion);
        }

        using (doc)
        {
            var root = doc.RootElement;
            var summary = GetString(root, "summary")
                ?? "Vaka belgeleri incelendi; ayrıntılı değerlendirme doktor incelemesindedir.";
            var confidence = Math.Clamp(GetDecimal(root, "confidence") ?? 0.3m, 0m, 1m);

            var findings = new List<LlmFinding>();
            if (root.TryGetProperty("findings", out var findingsElement) && findingsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in findingsElement.EnumerateArray())
                {
                    var description = GetString(f, "description");
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        continue;
                    }

                    Guid? sourceId = Guid.TryParse(GetString(f, "sourceDocumentId"), out var g) ? g : null;
                    findings.Add(new LlmFinding(description, sourceId));
                }
            }

            var differentials = new List<LlmDifferential>();
            if (root.TryGetProperty("differentials", out var diffElement) && diffElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var d in diffElement.EnumerateArray())
                {
                    var name = GetString(d, "name");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    var indexes = new List<int>();
                    if (d.TryGetProperty("sourceFindingIndexes", out var idx) && idx.ValueKind == JsonValueKind.Array)
                    {
                        indexes.AddRange(idx.EnumerateArray()
                            .Where(i => i.ValueKind == JsonValueKind.Number)
                            .Select(i => i.GetInt32()));
                    }

                    differentials.Add(new LlmDifferential(
                        name,
                        Math.Clamp(GetDecimal(d, "confidence") ?? 0m, 0m, 1m),
                        GetString(d, "riskLevel") ?? "Unknown",
                        indexes));
                }
            }

            return new LlmResult(summary, findings, differentials, confidence, model, promptVersion);
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                return trimmed[(firstNewline + 1)..lastFence].Trim();
            }
        }

        return trimmed;
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static decimal? GetDecimal(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : null;
}
