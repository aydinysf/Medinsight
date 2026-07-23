using MedInsight.Application.Abstractions.Repositories;

namespace MedInsight.Application.Patients;

public sealed class GetPatientQueryHandler(IPatientRepository patients, IUserRepository users)
{
    public async Task<PatientDto?> HandleAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        var user = await users.GetByIdAsync(patient.UserId, cancellationToken);
        return user is null
            ? null
            : new PatientDto(patient.Id, user.Id, user.FullName, user.Email, patient.DateOfBirth, patient.Sex, patient.CreatedAtUtc);
    }
}
