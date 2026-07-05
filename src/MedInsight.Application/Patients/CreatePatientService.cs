using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Domain.Entities;
using MedInsight.Domain.Enums;

namespace MedInsight.Application.Patients;

public sealed class CreatePatientService(IPatientRepository patients)
{
    public async Task<PatientDto> ExecuteAsync(PatientCreateDto dto, CancellationToken cancellationToken = default)
    {
        var patient = Patient.Create(dto.FullName, dto.BirthDate, dto.Sex ?? Sex.Unknown);

        patients.Add(patient);
        await patients.SaveChangesAsync(cancellationToken);

        return patient.ToDto();
    }
}
