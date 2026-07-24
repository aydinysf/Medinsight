using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;

namespace MedInsight.Application.Radiology;

/// <summary>
/// Arayüzde ana AI analizinden AYRI, "Deneysel — doğrulanmamış" etiketli blokta
/// gösterilir; disclaimer her kayıtta zorunlu olarak taşınır (ADR-010).
/// </summary>
public sealed record ImageFindingDto(
    Guid Id,
    Guid? StudyId,
    string ModelName,
    string ModelSource,
    string OutputType,
    string Description,
    string Disclaimer,
    DateTime CreatedAtUtc);

public sealed class GetImageFindingsQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<ImageFindingDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);
        return medicalCase.ImageFindings
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new ImageFindingDto(f.Id, f.StudyId, f.ModelName, f.ModelSource, f.OutputType, f.Description, f.Disclaimer, f.CreatedAtUtc))
            .ToList();
    }
}
