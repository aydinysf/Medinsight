using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Patients;

public sealed record RegisterPatient(
    [Required] [StringLength(200, MinimumLength = 2)] string FullName,
    [Required] [EmailAddress] [StringLength(320)] string Email,
    DateOnly? DateOfBirth,
    Sex? Sex);

public sealed record PatientDto(Guid Id, Guid UserId, string FullName, string Email, DateOnly? DateOfBirth, Sex Sex, DateTime CreatedAtUtc);

public sealed class RegisterPatientHandler(IUserRepository users, IPatientRepository patients)
{
    public async Task<PatientDto> HandleAsync(RegisterPatient command, CancellationToken cancellationToken = default)
    {
        if (await users.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new DomainException("Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.");
        }

        var user = User.Create(command.FullName, command.Email);
        var patient = Patient.Create(user.Id, command.DateOfBirth, command.Sex ?? Sex.Unknown);

        users.Add(user);
        patients.Add(patient);
        await patients.SaveChangesAsync(cancellationToken);

        return new PatientDto(patient.Id, user.Id, user.FullName, user.Email, patient.DateOfBirth, patient.Sex, patient.CreatedAtUtc);
    }
}
