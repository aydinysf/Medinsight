using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Common;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Doctors;

public sealed record RegisterDoctor(
    [Required] [StringLength(200, MinimumLength = 2)] string FullName,
    [Required] [EmailAddress] [StringLength(320)] string Email,
    [Required] [StringLength(128, MinimumLength = 8)] string Password,
    [Required] [StringLength(200, MinimumLength = 2)] string Specialty,
    [Required] [StringLength(100, MinimumLength = 2)] string LicenseNumber,
    [StringLength(100)] string? Title,
    [Range(0, 60)] int YearsOfExperience);

public sealed record DoctorDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string? Title,
    string Specialty,
    string LicenseNumber,
    int YearsOfExperience,
    VerificationStatus VerificationStatus,
    AvailabilityStatus EffectiveStatus,
    int ActiveCaseCount,
    int CapacityThreshold);

public sealed class RegisterDoctorHandler(IUserRepository users, IDoctorRepository doctors, IPasswordHasher passwordHasher)
{
    public async Task<DoctorDto> HandleAsync(RegisterDoctor command, CancellationToken cancellationToken = default)
    {
        if (await users.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new DomainException("Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.");
        }

        // Serbest kayıt yok: doktor Pending doğrulama durumuyla açılır (ADR-007).
        var user = User.Create(command.FullName, command.Email, UserRole.Doctor, passwordHasher.Hash(command.Password));
        var doctor = Doctor.Create(user.Id, command.Specialty, command.LicenseNumber, command.Title, command.YearsOfExperience);

        users.Add(user);
        doctors.Add(doctor);
        await doctors.SaveChangesAsync(cancellationToken);

        return doctor.ToDto(user.FullName);
    }
}

public static class DoctorMappings
{
    public static DoctorDto ToDto(this Doctor doctor, string fullName) =>
        new(
            doctor.Id,
            doctor.UserId,
            fullName,
            doctor.Title,
            doctor.Specialty,
            doctor.LicenseNumber,
            doctor.YearsOfExperience,
            doctor.VerificationStatus,
            doctor.EffectiveStatus,
            doctor.ActiveCaseCount,
            doctor.CapacityThreshold);
}
