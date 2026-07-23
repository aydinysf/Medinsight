using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Patients;

public sealed class GetPatientQueryHandler(IPatientRepository patients, IUserRepository users, ICurrentUser currentUser)
{
    public async Task<PatientDto?> HandleAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        // Kaynak bazlı yetki: hasta kendisi veya Admin (ADR-016).
        if (currentUser.Role != UserRole.Admin && patient.UserId != currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        var user = await users.GetByIdAsync(patient.UserId, cancellationToken);
        return user is null
            ? null
            : new PatientDto(patient.Id, user.Id, user.FullName, user.Email, patient.DateOfBirth, patient.Sex, patient.CreatedAtUtc);
    }
}
