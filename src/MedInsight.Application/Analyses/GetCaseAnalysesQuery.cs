using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Analyses;

public sealed record AiFindingDto(Guid Id, string Description, AiFindingSource Source, Guid? SourceDocumentId, string? Disclaimer);

public sealed record DifferentialDiagnosisDto(Guid Id, string Name, decimal ConfidenceScore, RiskLevel RiskLevel, IReadOnlyList<Guid> SourceFindingIds);

/// <summary>Doktora yönelik görünüm — güven skoru sayısal olarak yalnızca burada yer alır.</summary>
public sealed record AiAnalysisDto(
    Guid Id,
    Guid CaseId,
    string ModelVersion,
    string PromptVersion,
    decimal ConfidenceScore,
    string Summary,
    string PatientMessage,
    IReadOnlyList<AiFindingDto> Findings,
    IReadOnlyList<DifferentialDiagnosisDto> DifferentialDiagnoses,
    DateTime CreatedAtUtc);

public static class AiAnalysisMappings
{
    public static AiAnalysisDto ToDto(this AiAnalysis analysis) =>
        new(
            analysis.Id,
            analysis.CaseId,
            analysis.ModelVersion,
            analysis.PromptVersion,
            analysis.ConfidenceScore,
            analysis.Summary,
            analysis.PatientMessage,
            analysis.Findings.Select(f => new AiFindingDto(f.Id, f.Description, f.Source, f.SourceDocumentId, f.Disclaimer)).ToList(),
            analysis.DifferentialDiagnoses.Select(d => new DifferentialDiagnosisDto(d.Id, d.Name, d.ConfidenceScore, d.RiskLevel, d.SourceFindingIds)).ToList(),
            analysis.CreatedAtUtc);
}

public sealed class GetCaseAnalysesQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<AiAnalysisDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);
        return medicalCase.AiAnalyses
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => a.ToDto())
            .ToList();
    }
}
