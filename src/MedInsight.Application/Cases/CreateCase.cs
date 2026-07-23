using System.ComponentModel.DataAnnotations;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Cases;

public sealed record CreateCase(
    [Required] Guid PatientId,
    [Required] [StringLength(200, MinimumLength = 2)] string Title,
    [StringLength(2000)] string? Description,
    BodySystem? BodySystem);

public sealed record CaseDto(
    Guid Id,
    Guid PatientId,
    string Title,
    string? Description,
    BodySystem BodySystem,
    CaseStatus Status,
    RiskLevel RiskLevel,
    DateTime CreatedAtUtc);

public static class CaseMappings
{
    public static CaseDto ToDto(this Case medicalCase) =>
        new(
            medicalCase.Id,
            medicalCase.PatientId,
            medicalCase.Title,
            medicalCase.Description,
            medicalCase.BodySystem,
            medicalCase.Status,
            medicalCase.RiskLevel,
            medicalCase.CreatedAtUtc);
}

public sealed class CreateCaseHandler(IPatientRepository patients, ICaseRepository cases, ICurrentUser currentUser)
{
    /// <summary>Hasta bulunamazsa null döner (API 404'e eşler).</summary>
    public async Task<CaseDto?> HandleAsync(CreateCase command, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(command.PatientId, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        // Kaynak bazlı yetki: Patient yalnızca kendi adına vaka açabilir; Admin serbest (ADR-016).
        if (currentUser.Role != UserRole.Admin && patient.UserId != currentUser.UserId)
        {
            throw new ForbiddenAccessException("Yalnızca kendi adınıza vaka oluşturabilirsiniz.");
        }

        var medicalCase = Case.Create(
            patient.Id,
            patient.UserId,
            command.Title,
            command.BodySystem ?? BodySystem.Unknown,
            command.Description);

        cases.Add(medicalCase);
        await cases.SaveChangesAsync(cancellationToken);

        return medicalCase.ToDto();
    }
}
