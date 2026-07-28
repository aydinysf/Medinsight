using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace MedInsight.AIOrchestration;

public sealed class GeminiOptions
{
    public const string SectionName = "Ai:Gemini";

    /// <summary>Secrets manager / user-secrets'tan gelir — appsettings'e asla yazılmaz (security-architecture.md).</summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-2.5-flash";

    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
}

/// <summary>
/// Google Gemini implementasyonu (generateContent REST API).
/// Prompt-injection savunması korunur: belge içeriği yalnızca user içeriğine girer,
/// sistem talimatları sabit kalır. Model JSON şemasına zorlanır (responseSchema);
/// yine de ayrıştırma savunmacıdır — bozuk alanlar guardrail'lere takılacak şekilde
/// düşük güvenle işaretlenir. Sağlayıcı seçimi: Ai:Provider (DependencyInjection.cs).
/// </summary>
public sealed class GeminiLlmClient(HttpClient http, IOptions<GeminiOptions> options) : ILlmClient
{
    public const string PromptVersion = "hizir-gemini-prompt-v1";

    private const string OutputContract =
        "YANIT BİÇİMİ: Yalnızca geçerli JSON döndür, başka hiçbir metin yazma. Şema: " +
        "{\"summary\": string (doktora yönelik yorumsuz özet, Türkçe), " +
        "\"confidence\": number (0-1 arası; kanıt zayıfsa düşük ver), " +
        "\"findings\": [{\"description\": string, \"sourceDocumentId\": string}], " +
        "\"differentials\": [{\"name\": string, \"confidence\": number, \"riskLevel\": \"Low\"|\"Medium\"|\"High\", \"sourceFindingIndexes\": [int]}]}. " +
        "KURALLAR: Her bulgunun sourceDocumentId'si, bağlamdaki [BELGE:<guid>] başlığındaki guid olmalıdır; " +
        "belgeye dayandıramadığın bulguyu HİÇ yazma. differentials yalnızca olasılık sıralamasıdır, kesin tanı değildir; " +
        "sourceFindingIndexes findings dizisindeki 0 tabanlı indekslerdir. Emin değilsen differentials boş bırak.";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<LlmResult> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
        {
            throw new InvalidOperationException(
                "Ai:Gemini:ApiKey tanımlı değil — 'dotnet user-secrets set \"Ai:Gemini:ApiKey\" \"...\"' ile ekleyin.");
        }

        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = $"{request.SystemInstructions}\n\n{OutputContract}" } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = $"{request.ClinicalContext}\n\nGÖREV: {request.UserMessage}" } } },
            },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" },
        };

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{opts.Endpoint.TrimEnd('/')}/v1beta/models/{opts.Model}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(body, SerializerOptions), Encoding.UTF8, "application/json"),
        };
        message.Headers.Add("x-goog-api-key", opts.ApiKey);

        using var response = await http.SendAsync(message, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // 429 = ücretsiz katman dakika limiti; outbox yeniden dener (at-least-once).
            throw new HttpRequestException($"Gemini isteği başarısız ({(int)response.StatusCode}): {Truncate(payload, 500)}");
        }

        var text = ExtractCandidateText(payload)
            ?? throw new InvalidOperationException("Gemini yanıtında metin içeriği yok (güvenlik bloğu olabilir).");

        return ParseResult(text, opts.Model);
    }

    /// <summary>candidates[0].content.parts[*].text birleştirilir.</summary>
    private static string? ExtractCandidateText(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return null;
        }

        if (!candidates[0].TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts))
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var t))
            {
                builder.Append(t.GetString());
            }
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }

    /// <summary>Savunmacı ayrıştırma — model şemadan saparsa çıktı guardrail'lere düşük güvenle gider.</summary>
    public static LlmResult ParseResult(string text, string model)
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
                [], [], 0.2m, model, PromptVersion);
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

            return new LlmResult(summary, findings, differentials, confidence, model, PromptVersion);
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

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
