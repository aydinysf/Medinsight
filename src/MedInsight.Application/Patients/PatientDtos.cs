using System.ComponentModel.DataAnnotations;
using MedInsight.Domain.Entities;
using MedInsight.Domain.Enums;

namespace MedInsight.Application.Patients;

public sealed record PatientDto(Guid Id, string FullName, DateOnly? BirthDate, Sex Sex, DateTime CreatedAtUtc);

public sealed record PatientCreateDto(
    [Required] [StringLength(200, MinimumLength = 2)] string FullName,
    DateOnly? BirthDate,
    Sex? Sex);

public static class PatientMappings
{
    public static PatientDto ToDto(this Patient patient) =>
        new(patient.Id, patient.FullName, patient.BirthDate, patient.Sex, patient.CreatedAtUtc);
}
