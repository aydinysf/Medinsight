using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

/// <summary>Snapshot'ı kimin tetiklediği (bkz. health-route-versioning.md).</summary>
public enum RouteTrigger
{
    System = 0,
    AI = 1,
    Doctor = 2,
    Patient = 3,
}

/// <summary>
/// "HEAD" — hastanın güncel rotası (ADR-002, Git modeli). Değiştirilemez geçmiş
/// HealthRouteSnapshot zincirindedir; current state her zaman tam olarak bir tanedir.
/// </summary>
public sealed class HealthRoute : Entity
{
    private HealthRoute()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid CurrentVersionId { get; private set; }

    public string CurrentStatus { get; private set; } = null!;

    public string NextStep { get; private set; } = null!;

    public RiskLevel RiskLevel { get; private set; }

    internal static HealthRoute Create(Guid caseId, HealthRouteSnapshot initial)
    {
        var route = new HealthRoute { CaseId = caseId };
        route.MoveTo(initial);
        return route;
    }

    internal void MoveTo(HealthRouteSnapshot snapshot)
    {
        CurrentVersionId = snapshot.Id;
        CurrentStatus = snapshot.Status;
        NextStep = snapshot.NextStep;
        RiskLevel = snapshot.RiskLevel;
    }
}

/// <summary>Append-only "commit" — asla güncellenmez veya silinmez (ADR-002).</summary>
public sealed class HealthRouteSnapshot : Entity
{
    private HealthRouteSnapshot()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid? PreviousVersionId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Status { get; private set; } = null!;

    public string NextStep { get; private set; } = null!;

    public RiskLevel RiskLevel { get; private set; }

    public RouteTrigger TriggeredBy { get; private set; }

    /// <summary>Hangi AIAnalysis, Consultation veya Treatment'tan geldi.</summary>
    public Guid? TriggerSourceId { get; private set; }

    public string Reason { get; private set; } = null!;

    internal static HealthRouteSnapshot Create(
        Guid caseId,
        Guid? previousVersionId,
        int versionNumber,
        string status,
        string nextStep,
        RiskLevel riskLevel,
        RouteTrigger triggeredBy,
        Guid? triggerSourceId,
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        ArgumentException.ThrowIfNullOrWhiteSpace(nextStep);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new HealthRouteSnapshot
        {
            CaseId = caseId,
            PreviousVersionId = previousVersionId,
            VersionNumber = versionNumber,
            Status = status,
            NextStep = nextStep,
            RiskLevel = riskLevel,
            TriggeredBy = triggeredBy,
            TriggerSourceId = triggerSourceId,
            Reason = reason,
        };
    }
}
