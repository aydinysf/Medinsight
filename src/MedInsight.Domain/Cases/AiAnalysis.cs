using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

/// <summary>
/// AI ön analizi. Summary doktora, PatientMessage hastaya yöneliktir —
/// hastaya ham güven skoru asla gösterilmez (bkz. ai/confidence-management.md).
/// ModelVersion + PromptVersion zorunlu ve silinmez (bkz. backend/versioning.md).
/// </summary>
public sealed class AiAnalysis : Entity
{
    private readonly List<AiFinding> _findings = [];
    private readonly List<DifferentialDiagnosis> _differentialDiagnoses = [];

    private AiAnalysis()
    {
    }

    public Guid CaseId { get; private set; }

    public string ModelVersion { get; private set; } = null!;

    public string PromptVersion { get; private set; } = null!;

    public decimal ConfidenceScore { get; private set; }

    public string Summary { get; private set; } = null!;

    public string PatientMessage { get; private set; } = null!;

    public IReadOnlyCollection<AiFinding> Findings => _findings.AsReadOnly();

    public IReadOnlyCollection<DifferentialDiagnosis> DifferentialDiagnoses => _differentialDiagnoses.AsReadOnly();

    internal static AiAnalysis Create(Guid caseId, string modelVersion, string promptVersion, decimal confidenceScore, string summary, string patientMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(promptVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientMessage);

        return new AiAnalysis
        {
            CaseId = caseId,
            ModelVersion = modelVersion,
            PromptVersion = promptVersion,
            ConfidenceScore = confidenceScore,
            Summary = summary,
            PatientMessage = patientMessage,
        };
    }

    internal AiFinding AddFinding(string description, AiFindingSource source, Guid? sourceDocumentId, string? disclaimer)
    {
        // ADR-010: klinik olarak doğrulanmamış görüntü modeli bulgusu zorunlu disclaimer taşır.
        if (source == AiFindingSource.OpenSourceImageModel && string.IsNullOrWhiteSpace(disclaimer))
        {
            throw new DomainException("OpenSourceImageModel kaynaklı bulgu zorunlu disclaimer olmadan eklenemez (ADR-010).");
        }

        var finding = AiFinding.Create(CaseId, Id, description, source, sourceDocumentId, disclaimer);
        _findings.Add(finding);
        return finding;
    }

    internal DifferentialDiagnosis AddDifferentialDiagnosis(string name, decimal confidenceScore, RiskLevel riskLevel, IReadOnlyList<Guid> sourceFindingIds)
    {
        // Invariant 3 (case-aggregate-root.md): kaynaksız tanı adayı üretilemez.
        if (sourceFindingIds.Count == 0)
        {
            throw new DomainException("DifferentialDiagnosis en az bir AIFindings referansı olmadan eklenemez (kaynak izlenebilirliği).");
        }

        foreach (var findingId in sourceFindingIds)
        {
            var finding = _findings.FirstOrDefault(f => f.Id == findingId)
                ?? throw new DomainException("DifferentialDiagnosis yalnızca bu analizdeki bulgulara referans verebilir.");

            // ADR-010: doğrulanmamış görüntü modeli çıktısı asla DifferentialDiagnosis'u besleyemez.
            if (finding.Source == AiFindingSource.OpenSourceImageModel)
            {
                throw new DomainException("OpenSourceImageModel kaynaklı bulgu DifferentialDiagnosis'u besleyemez (ADR-010).");
            }
        }

        var diagnosis = DifferentialDiagnosis.Create(CaseId, Id, name, confidenceScore, riskLevel, sourceFindingIds);
        _differentialDiagnoses.Add(diagnosis);
        return diagnosis;
    }
}

/// <summary>Bulgunun kaynağı (bkz. case-aggregate-root.md, AIFindings.Source).</summary>
public enum AiFindingSource
{
    LLMTextAnalysis = 0,
    OpenSourceImageModel = 1,
}

/// <summary>Ham gözlem — yorum içermez ("sol frontal lobda 2.3cm kitle").</summary>
public sealed class AiFinding : Entity
{
    private AiFinding()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid AnalysisId { get; private set; }

    public string Description { get; private set; } = null!;

    public AiFindingSource Source { get; private set; }

    public Guid? SourceDocumentId { get; private set; }

    public string? Disclaimer { get; private set; }

    internal static AiFinding Create(Guid caseId, Guid analysisId, string description, AiFindingSource source, Guid? sourceDocumentId, string? disclaimer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new AiFinding
        {
            CaseId = caseId,
            AnalysisId = analysisId,
            Description = description,
            Source = source,
            SourceDocumentId = sourceDocumentId,
            Disclaimer = disclaimer,
        };
    }
}

/// <summary>Bulguların sentezinden üretilen olası tanı adayı — her zaman confidence taşır.</summary>
public sealed class DifferentialDiagnosis : Entity
{
    private DifferentialDiagnosis()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid AnalysisId { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal ConfidenceScore { get; private set; }

    public RiskLevel RiskLevel { get; private set; }

    public List<Guid> SourceFindingIds { get; private set; } = [];

    internal static DifferentialDiagnosis Create(Guid caseId, Guid analysisId, string name, decimal confidenceScore, RiskLevel riskLevel, IReadOnlyList<Guid> sourceFindingIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new DifferentialDiagnosis
        {
            CaseId = caseId,
            AnalysisId = analysisId,
            Name = name,
            ConfidenceScore = confidenceScore,
            RiskLevel = riskLevel,
            SourceFindingIds = [.. sourceFindingIds],
        };
    }
}
