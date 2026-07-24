using System.Text;
using MedInsight.Domain.Cases;

namespace MedInsight.AIOrchestration.Pipeline;

/// <summary>Katman 1 — Intent Detection. MVP: tek intent (vaka analizi).</summary>
public enum AnalysisIntent
{
    AnalyzeCase = 0,
}

public sealed class IntentDetector
{
    public AnalysisIntent Detect(Case medicalCase) => AnalysisIntent.AnalyzeCase;
}

/// <summary>Katman 2 — Planner: hangi adımlar gerekli.</summary>
public sealed class AnalysisPlanner
{
    public IReadOnlyList<string> Plan(AnalysisIntent intent) =>
        ["GatherCaseData", "BuildContext", "Reason", "ApplyGuardrails", "ComposeResponse"];
}

/// <summary>
/// Katman 3 — Agent Selection. MVP'de her zaman "Hizir" (tek genel ajan);
/// Post-MVP'de gerçek yönlendirmeye dönüşür (bkz. ai/ai-agent-ecosystem.md).
/// </summary>
public sealed class AgentSelector
{
    public string Select(AnalysisIntent intent) => "Hizir";
}

/// <summary>Katman 4 — Tool Calling: vaka verilerini toplar (belgeler, DICOM özetleri).</summary>
public sealed class CaseToolInvoker
{
    public sealed record CaseData(
        IReadOnlyList<MedicalDocument> DocumentsWithText,
        IReadOnlyList<DicomStudy> GroupedStudies);

    public CaseData Gather(Case medicalCase) =>
        new(
            medicalCase.Documents.Where(d => !string.IsNullOrWhiteSpace(d.ExtractedText)).ToList(),
            medicalCase.DicomStudies.Where(s => s.IsGrouped).ToList());
}

/// <summary>
/// Katman 5 — Memory/Context. PII minimizasyonu burada: modele yalnızca klinik
/// veri gider — ad, e-posta, kimlik bilgisi asla (bkz. guardrails-and-boundaries.md).
/// Belge içerikleri yalnızca context'e girer (prompt-injection savunması).
/// </summary>
public sealed class MemoryContextBuilder
{
    public string Build(Case medicalCase, CaseToolInvoker.CaseData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[VAKA] Sistem: {medicalCase.BodySystem}. Önceki analiz sayısı: {medicalCase.AiAnalyses.Count}.");

        if (medicalCase.HealthRoute is not null)
        {
            builder.AppendLine($"[ROTA] Mevcut durum: {medicalCase.HealthRoute.CurrentStatus}");
        }

        foreach (var study in data.GroupedStudies)
        {
            builder.AppendLine("---");
            builder.AppendLine($"[DICOM] {study.Modality} çalışması, {study.Series.Count} seri, {study.Series.Sum(s => s.SliceCount)} kesit.");
        }

        foreach (var document in data.DocumentsWithText)
        {
            builder.AppendLine("---");
            builder.AppendLine($"[BELGE:{document.Id}] {document.ExtractedText}");
        }

        return builder.ToString();
    }
}

/// <summary>Katman 6 — Reasoning: modele çağrı. Sistem talimatları sabittir, belge içeriği buraya karışamaz.</summary>
public sealed class ReasoningEngine(ILlmClient llmClient)
{
    private const string SystemInstructions =
        "Sen Hızır'sın: MedInsight klinik karar destek ajanı. KESİN KURALLAR: " +
        "(1) Tanı koymazsın. (2) İlaç dozu veya tedavi kararı önermezsin. " +
        "(3) Her tıbbi ifade bir belgeye dayanmalıdır; kaynaksız iddia üretme. " +
        "(4) Emin olmadığında belirsizliği açıkça söyle. Bulguları yorumsuz aktar.";

    public Task<LlmResult> ReasonAsync(string clinicalContext, CancellationToken cancellationToken) =>
        llmClient.CompleteAsync(
            new LlmRequest(SystemInstructions, clinicalContext, "Vakadaki belgeleri incele ve ön analiz üret."),
            cancellationToken);
}
