using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace MedInsight.AIOrchestration;

public sealed class OpenAiCompatibleOptions
{
    public const string SectionName = "Ai:OpenAiCompatible";

    /// <summary>Secrets manager / user-secrets'tan gelir — appsettings'e asla yazılmaz.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>chat/completions kökü. Kimi: https://api.moonshot.ai/v1 · DeepSeek: https://api.deepseek.com/v1</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// OpenAI-uyumlu chat/completions istemcisi — Kimi (Moonshot), DeepSeek, OpenAI ve
/// benzeri sağlayıcılar tek sınıfla bağlanır; Ai:Provider = Kimi | DeepSeek |
/// OpenAiCompatible varsayılan BaseUrl/Model'i belirler (DependencyInjection.cs).
/// Prompt-injection savunması ve JSON çıktı sözleşmesi Gemini istemcisiyle ortaktır.
/// </summary>
public sealed class OpenAiCompatibleLlmClient(HttpClient http, IOptions<OpenAiCompatibleOptions> options) : ILlmClient
{
    public const string PromptVersion = "hizir-openai-compat-prompt-v1";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<LlmResult> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
        {
            throw new InvalidOperationException(
                "Ai:OpenAiCompatible:ApiKey tanımlı değil — 'dotnet user-secrets set \"Ai:OpenAiCompatible:ApiKey\" \"...\"' ile ekleyin.");
        }

        if (string.IsNullOrWhiteSpace(opts.BaseUrl) || string.IsNullOrWhiteSpace(opts.Model))
        {
            throw new InvalidOperationException("Ai:OpenAiCompatible:BaseUrl ve Model tanımlı olmalı.");
        }

        var body = new
        {
            model = opts.Model,
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = $"{request.SystemInstructions}\n\n{LlmJsonContract.OutputContract}" },
                new { role = "user", content = $"{request.ClinicalContext}\n\nGÖREV: {request.UserMessage}" },
            },
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"{opts.BaseUrl.TrimEnd('/')}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body, SerializerOptions), Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);

        using var response = await http.SendAsync(message, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // 429 = kota/kredi; outbox yeniden dener (at-least-once).
            throw new HttpRequestException($"LLM isteği başarısız ({(int)response.StatusCode}, {opts.BaseUrl}): {Truncate(payload, 500)}");
        }

        var text = ExtractMessageContent(payload)
            ?? throw new InvalidOperationException("LLM yanıtında mesaj içeriği yok.");

        return LlmJsonContract.ParseResult(text, opts.Model, PromptVersion);
    }

    /// <summary>Hızır sohbeti (ADR-018): bağlam ilk user dönüşünde, sistem talimatı sabit.</summary>
    public async Task<string> ChatAsync(LlmChatRequest request, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
        {
            throw new InvalidOperationException("Ai:OpenAiCompatible:ApiKey tanımlı değil.");
        }

        var messages = new List<object>
        {
            new { role = "system", content = request.SystemInstructions },
            new { role = "user", content = $"[BAĞLAM — vaka verileri]\n{request.ClinicalContext}" },
            new { role = "assistant", content = "Bağlamı aldım; hastanın sorularını bu vaka verilerine ve kurallarıma göre yanıtlayacağım." },
        };
        messages.AddRange(request.History.Select(t => (object)new { role = t.Role == "assistant" ? "assistant" : "user", content = t.Content }));
        messages.Add(new { role = "user", content = request.UserMessage });

        var body = new { model = opts.Model, temperature = 0.4, messages };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"{opts.BaseUrl.TrimEnd('/')}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body, SerializerOptions), Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);

        using var response = await http.SendAsync(message, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"LLM sohbet isteği başarısız ({(int)response.StatusCode}): {Truncate(payload, 500)}");
        }

        return ExtractMessageContent(payload)
            ?? throw new InvalidOperationException("LLM sohbet yanıtında mesaj içeriği yok.");
    }

    /// <summary>choices[0].message.content.</summary>
    private static string? ExtractMessageContent(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        {
            return null;
        }

        if (!choices[0].TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content))
        {
            return null;
        }

        return content.ValueKind == JsonValueKind.String ? content.GetString() : null;
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
