using MedInsight.Domain.Common;

namespace MedInsight.Domain.Cases;

public sealed class CaseMember : Entity
{
    private CaseMember()
    {
    }

    public Guid CaseId { get; private set; }

    public Guid UserId { get; private set; }

    public CaseRole Role { get; private set; }

    public PermissionLevel PermissionLevel { get; private set; }

    internal static CaseMember Create(Guid caseId, Guid userId, CaseRole role, PermissionLevel permissionLevel)
    {
        return new CaseMember
        {
            CaseId = caseId,
            UserId = userId,
            Role = role,
            PermissionLevel = permissionLevel,
        };
    }
}

public enum CaseRole
{
    Patient = 0,
    Caregiver = 1,
    Doctor = 2,
}

public enum PermissionLevel
{
    ReadOnly = 0,
    Contribute = 1,
    Manage = 2,
}
