using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Entities;
using MedInsight.Domain.Enums;

namespace MedInsight.Application.MedicalCases;

public sealed class CreateMedicalCaseService(IPatientRepository patients, IMedicalCaseRepository medicalCases)
{
    /// <summary>Returns null when the patient does not exist.</summary>
    public async Task<MedicalCaseDto?> ExecuteAsync(Guid patientId, MedicalCaseCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (!await patients.ExistsAsync(patientId, cancellationToken))
        {
            return null;
        }

        var medicalCase = MedicalCase.Create(patientId, dto.Title, dto.BodySystem ?? BodySystem.Unknown, dto.Description);

        medicalCases.Add(medicalCase);
        await medicalCases.SaveChangesAsync(cancellationToken);

        return medicalCase.ToDto();
    }
}
