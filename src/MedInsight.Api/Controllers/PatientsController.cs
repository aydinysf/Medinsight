using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Patients;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("patients")]
public sealed class PatientsController(CreatePatientService createPatient, IPatientRepository patients) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PatientDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientDto>> Create(PatientCreateDto dto, CancellationToken cancellationToken)
    {
        var patient = await createPatient.ExecuteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PatientDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : patient.ToDto();
    }
}
