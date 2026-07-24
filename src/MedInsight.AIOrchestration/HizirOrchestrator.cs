using MedInsight.AIOrchestration.Pipeline;
using MedInsight.Domain.Cases;

namespace MedInsight.AIOrchestration;

public sealed record HizirAnalysisResult(
    string ModelVersion,
    string PromptVersion,
    decimal ConfidenceScore,
    bool LowConfidence,
    string Summary,
    string PatientMessage,
    IReadOnlyList<AiFindingInput> Findings,
    IReadOnlyList<DifferentialDiagnosisInput> DifferentialDiagnoses);

/// <summary>
/// 7 katmanlı orkestrasyon (bkz. ai/ai-orchestration-flow.md). MVP'de tek model
/// çağrısı gibi görünür ama tüm katmanlar mevcut — Agent Selection her zaman
/// "Hizir" seçer; çoklu-ajan geçişi genişlemedir, yeniden yazım değil.
/// </summary>
public sealed class HizirOrchestrator(
    IntentDetector intentDetector,
    AnalysisPlanner planner,
    AgentSelector agentSelector,
    CaseToolInvoker toolInvoker,
    MemoryContextBuilder contextBuilder,
    ReasoningEngine reasoningEngine,
    Guardrails guardrails,
    ResponseComposer responseComposer)
{
    public async Task<HizirAnalysisResult> AnalyzeAsync(Case medicalCase, CancellationToken cancellationToken = default)
    {
        // 1-2-3: intent → plan → ajan seçimi (MVP: hep Hizir)
        var intent = intentDetector.Detect(medicalCase);
        _ = planner.Plan(intent);
        _ = agentSelector.Select(intent);

        // 4-5: araçlar → PII-minimize bağlam
        var caseData = toolInvoker.Gather(medicalCase);
        var context = contextBuilder.Build(medicalCase, caseData);

        // 6: reasoning
        var result = await reasoningEngine.ReasonAsync(context, cancellationToken);

        // Guardrails — üç kapı
        var lowConfidence = guardrails.IsLowConfidence(result.ConfidenceScore);
        var traceableFindings = guardrails.EnforceSourceTraceability(result.Findings);
        var summary = guardrails.EnforceScope(result.Summary);

        var findingInputs = traceableFindings
            .Select(f => new AiFindingInput(f.Description, AiFindingSource.LLMTextAnalysis, f.SourceDocumentId))
            .ToList();

        // Kaynak izlenebilirliği: elenen bulgulara referans veren tanı adayları da elenir.
        var keptIndexes = result.Findings
            .Select((finding, index) => (finding, index))
            .Where(x => x.finding.SourceDocumentId is not null)
            .Select((x, newIndex) => (oldIndex: x.index, newIndex))
            .ToDictionary(x => x.oldIndex, x => x.newIndex);

        var differentialInputs = new List<DifferentialDiagnosisInput>();
        foreach (var differential in result.Differentials)
        {
            if (guardrails.ViolatesScope(differential.Name) || !differential.SourceFindingIndexes.All(keptIndexes.ContainsKey))
            {
                continue;
            }

            var risk = Enum.TryParse<RiskLevel>(differential.RiskLevel, ignoreCase: true, out var parsed) ? parsed : RiskLevel.Unknown;
            differentialInputs.Add(new DifferentialDiagnosisInput(
                differential.Name,
                differential.ConfidenceScore,
                risk,
                differential.SourceFindingIndexes.Select(i => keptIndexes[i]).ToList()));
        }

        // 7: persona/response katmanı
        var patientMessage = responseComposer.ComposePatientMessage(result, lowConfidence);

        return new HizirAnalysisResult(
            result.ModelVersion,
            result.PromptVersion,
            result.ConfidenceScore,
            lowConfidence,
            summary,
            patientMessage,
            findingInputs,
            differentialInputs);
    }
}
