using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Cases;

/// <summary>FollowUp → Closed: doktor (vaka üyesi) veya admin kapatır (state machine dokümanı).</summary>
public sealed class CloseCaseHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<CaseDto?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        if (currentUser.Role != UserRole.Admin &&
            (currentUser.Role != UserRole.Doctor || medicalCase.Members.All(m => m.UserId != currentUser.UserId)))
        {
            throw new ForbiddenAccessException("Vakayı yalnızca vakada görevli doktor veya admin kapatabilir.");
        }

        medicalCase.Close();
        await cases.SaveChangesAsync(cancellationToken);
        return medicalCase.ToDto();
    }
}

public sealed record ReopenCase([Required] [StringLength(1000, MinimumLength = 3)] string Reason);

/// <summary>Closed → FollowUp: hasta (Manage üyesi) veya admin; geçmiş korunur, Draft'a dönülmez.</summary>
public sealed class ReopenCaseHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    public async Task<CaseDto?> HandleAsync(Guid caseId, ReopenCase command, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        if (currentUser.Role != UserRole.Admin)
        {
            var member = medicalCase.Members.FirstOrDefault(m => m.UserId == currentUser.UserId);
            if (member is null || member.PermissionLevel < PermissionLevel.Manage)
            {
                throw new ForbiddenAccessException("Vakayı yalnızca hasta (Manage üyesi) veya admin yeniden açabilir.");
            }
        }

        medicalCase.Reopen(command.Reason);
        await cases.SaveChangesAsync(cancellationToken);
        return medicalCase.ToDto();
    }
}
