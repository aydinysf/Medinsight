using MedInsight.AIOrchestration;

namespace MedInsight.AIOrchestration.Tests;

/// <summary>
/// Ortak JSON sözleşmesi ayrıştırıcısı (Gemini + Kimi/DeepSeek/OpenAI-uyumlu istemciler)
/// savunmacıdır: bozuk model çıktısı bulgu üretmez, düşük güvenle doktora düşer.
/// </summary>
public class LlmJsonContractTests
{
    private const string Prompt = "test-prompt-v1";
    private static readonly Guid DocId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void Gecerli_json_tum_alanlariyla_ayristirilir()
    {
        var json = $$"""
            {
              "summary": "MR raporunda sinyal değişikliği bildirilmiş.",
              "confidence": 0.81,
              "findings": [{"description": "Sol frontal bölgede sinyal değişikliği", "sourceDocumentId": "{{DocId}}"}],
              "differentials": [{"name": "Demiyelinizan süreç olasılığı", "confidence": 0.4, "riskLevel": "Medium", "sourceFindingIndexes": [0]}]
            }
            """;

        var result = LlmJsonContract.ParseResult(json, "model-x", Prompt);

        Assert.Equal(0.81m, result.ConfidenceScore);
        Assert.Single(result.Findings);
        Assert.Equal(DocId, result.Findings[0].SourceDocumentId);
        Assert.Single(result.Differentials);
        Assert.Equal("Medium", result.Differentials[0].RiskLevel);
        Assert.Equal([0], result.Differentials[0].SourceFindingIndexes);
        Assert.Equal("model-x", result.ModelVersion);
        Assert.Equal(Prompt, result.PromptVersion);
    }

    [Fact]
    public void Kod_bloguna_sarili_json_ayristirilir()
    {
        var text = "```json\n{\"summary\": \"Özet\", \"confidence\": 0.7, \"findings\": [], \"differentials\": []}\n```";

        var result = LlmJsonContract.ParseResult(text, "model-x", Prompt);

        Assert.Equal("Özet", result.Summary);
        Assert.Equal(0.7m, result.ConfidenceScore);
    }

    [Fact]
    public void Bozuk_json_bulgu_uretmez_ve_dusuk_guvenle_doner()
    {
        var result = LlmJsonContract.ParseResult("Elbette! İşte analiz: hasta kesin MS.", "model-x", Prompt);

        Assert.Empty(result.Findings);
        Assert.Empty(result.Differentials);
        Assert.True(result.ConfidenceScore < 0.6m); // guardrail eşiği altı → doktor önceliği yükselir
    }

    [Fact]
    public void Gecersiz_guid_kaynak_null_olur_ve_guardrail_eler()
    {
        var json = """
            {"summary": "s", "confidence": 0.9,
             "findings": [{"description": "Kaynaksız iddia", "sourceDocumentId": "belge-yok"}],
             "differentials": []}
            """;

        var result = LlmJsonContract.ParseResult(json, "model-x", Prompt);

        Assert.Single(result.Findings);
        Assert.Null(result.Findings[0].SourceDocumentId); // EnforceSourceTraceability bunu eler
    }

    [Fact]
    public void Guven_skoru_0_1_araligina_sikistirilir()
    {
        var json = """{"summary": "s", "confidence": 7.5, "findings": [], "differentials": []}""";

        var result = LlmJsonContract.ParseResult(json, "model-x", Prompt);

        Assert.Equal(1m, result.ConfidenceScore);
    }

    [Fact]
    public void Eksik_alanlar_varsayilanlarla_tolere_edilir()
    {
        var result = LlmJsonContract.ParseResult("{}", "model-x", Prompt);

        Assert.NotEmpty(result.Summary);
        Assert.Empty(result.Findings);
        Assert.True(result.ConfidenceScore < 0.6m);
    }
}
