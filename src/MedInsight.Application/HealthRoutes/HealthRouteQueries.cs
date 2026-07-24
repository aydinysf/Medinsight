using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Domain.Cases;

namespace MedInsight.Application.HealthRoutes;

public sealed record HealthRouteDto(Guid CaseId, Guid CurrentVersionId, string CurrentStatus, string NextStep, RiskLevel RiskLevel);

public sealed record HealthRouteSnapshotDto(
    Guid Id,
    Guid? PreviousVersionId,
    int VersionNumber,
    string Status,
    string NextStep,
    RiskLevel RiskLevel,
    RouteTrigger TriggeredBy,
    Guid? TriggerSourceId,
    string Reason,
    DateTime CreatedAtUtc);

public sealed class GetHealthRouteQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<HealthRouteDto?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase?.HealthRoute is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);
        var route = medicalCase.HealthRoute;
        return new HealthRouteDto(medicalCase.Id, route.CurrentVersionId, route.CurrentStatus, route.NextStep, route.RiskLevel);
    }
}

public sealed class GetHealthRouteSnapshotsQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<HealthRouteSnapshotDto>?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        GetCaseQueryHandler.EnsureCanAccess(medicalCase, currentUser);
        return medicalCase.HealthRouteSnapshots
            .OrderByDescending(s => s.VersionNumber)
            .Select(s => new HealthRouteSnapshotDto(
                s.Id, s.PreviousVersionId, s.VersionNumber, s.Status, s.NextStep, s.RiskLevel, s.TriggeredBy, s.TriggerSourceId, s.Reason, s.CreatedAtUtc))
            .ToList();
    }
}
