using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.Documents;

public sealed record CaseDocumentDto(
    Guid Id,
    string Title,
    DocumentType Type,
    DocumentStatus Status,
    string? OriginalFileName,
    long SizeBytes,
    DateTime CreatedAtUtc);

public sealed class GetCaseDocumentsQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<CaseDocumentDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);

        return medicalCase.Documents
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new CaseDocumentDto(d.Id, d.Title, d.Type, d.Status, d.OriginalFileName, d.SizeBytes, d.CreatedAtUtc))
            .ToList();
    }
}
