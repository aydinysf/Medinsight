using System.ComponentModel.DataAnnotations;
using MedInsight.Domain.Entities;
using MedInsight.Domain.Enums;

namespace MedInsight.Application.MedicalCases;

public sealed record MedicalCaseDto(
    Guid Id,
    Guid PatientId,
    string Title,
    string? Description,
    BodySystem BodySystem,
    MedicalCaseStatus Status,
    DateTime CreatedAtUtc);

public sealed record MedicalCaseCreateDto(
    [Required] [StringLength(200, MinimumLength = 2)] string Title,
    [StringLength(2000)] string? Description,
    BodySystem? BodySystem);

public static class MedicalCaseMappings
{
    public static MedicalCaseDto ToDto(this MedicalCase medicalCase) =>
        new(
            medicalCase.Id,
            medicalCase.PatientId,
            medicalCase.Title,
            medicalCase.Description,
            medicalCase.BodySystem,
            medicalCase.Status,
            medicalCase.CreatedAtUtc);
}
