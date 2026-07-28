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
/// sistem talimatları sabit kalır. Çıktı sözleşmesi ve savunmacı ayrıştırma
/// LlmJsonContract'ta — sağlayıcılar arasında ortak. Sağlayıcı seçimi: Ai:Provider.
/// </summary>
public sealed class GeminiLlmClient(HttpClient http, IOptions<GeminiOptions> options) : ILlmClient
{
    public const string PromptVersion = "hizir-gemini-prompt-v1";

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
            systemInstruction = new { parts = new[] { new { text = $"{request.SystemInstructions}\n\n{LlmJsonContract.OutputContract}" } } },
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
            // 429 = kota/kredi; outbox yeniden dener (at-least-once).
            throw new HttpRequestException($"Gemini isteği başarısız ({(int)response.StatusCode}): {Truncate(payload, 500)}");
        }

        var text = ExtractCandidateText(payload)
            ?? throw new InvalidOperationException("Gemini yanıtında metin içeriği yok (güvenlik bloğu olabilir).");

        return LlmJsonContract.ParseResult(text, opts.Model, PromptVersion);
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

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
