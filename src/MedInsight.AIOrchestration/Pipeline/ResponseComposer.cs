namespace MedInsight.AIOrchestration.Pipeline;

/// <summary>
/// Katman 7 — karakter/persona katmanı (bkz. ai/hizir-personality.md):
/// sade dil, sayısal risk ifadesi yok, belirsizlik gizlenmez, her yüksek
/// risk/belirsizlik ifadesi somut bir sonraki adımla eşlenir.
/// </summary>
public sealed class ResponseComposer(Guardrails guardrails)
{
    public string ComposePatientMessage(LlmResult result, bool lowConfidence)
    {
        var lead = result.Findings.Count > 0
            ? "Belgelerini inceledim. Öne çıkan bilgileri doktorunun görebileceği şekilde derledim."
            : "Belgelerini aldım ama içlerinden okunabilir bir metin çıkaramadım.";

        var uncertainty = lowConfidence
            ? " Bu değerlendirmeden tam olarak emin değilim — doktorunun incelemesi önemli, vakan öncelikli olarak inceleme sırasına alındı."
            : " Bir sonraki adım: doktorun vakanı inceleyecek.";

        var message = lead + uncertainty + " Sorularını doktor görüşmesine not etmeni öneririm.";

        // Kapsam kapısı persona çıktısına da uygulanır — çift kontrol (bkz. ai-orchestration-flow.md).
        return guardrails.EnforceScope(message);
    }
}
