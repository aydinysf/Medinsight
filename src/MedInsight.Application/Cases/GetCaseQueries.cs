using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Cases;

public sealed class GetCaseQueryHandler(ICaseRepository cases, ICurrentUser currentUser)
{
    /// <summary>Vaka yoksa null; üye/Admin değilse ForbiddenAccessException.</summary>
    public async Task<CaseDto?> HandleAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var medicalCase = await cases.GetByIdAsync(caseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        EnsureCanAccess(medicalCase, currentUser);
        return medicalCase.ToDto();
    }

    /// <summary>Kaynak bazlı yetki (katman 2): vaka üyeliği veya Admin.</summary>
    internal static void EnsureCanAccess(Case medicalCase, ICurrentUser currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return;
        }

        if (medicalCase.Members.All(m => m.UserId != currentUser.UserId))
        {
            throw new ForbiddenAccessException("Bu vakaya erişim yetkiniz yok — vaka üyesi değilsiniz.");
        }
    }
}

public sealed class GetPatientCasesQueryHandler(IPatientRepository patients, ICaseRepository cases, ICurrentUser currentUser)
{
    /// <summary>Hasta yoksa null. Admin ve hastanın kendisi tümünü, diğerleri yalnızca üyesi olduğu vakaları görür.</summary>
    public async Task<IReadOnlyList<CaseDto>?> HandleAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        var result = await cases.GetByPatientIdAsync(patientId, cancellationToken);

        if (currentUser.Role != UserRole.Admin && patient.UserId != currentUser.UserId)
        {
            result = result.Where(c => c.Members.Any(m => m.UserId == currentUser.UserId)).ToList();
        }

        return result.Select(c => c.ToDto()).ToList();
    }
}
