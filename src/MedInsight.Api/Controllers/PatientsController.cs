using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Cases;
using MedInsight.Application.Patients;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/patients")]
public sealed class PatientsController(
    RegisterPatientHandler registerPatient,
    GetPatientQueryHandler getPatient,
    IPatientRepository patients,
    ICaseRepository cases) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PatientDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientDto>> Register(RegisterPatient command, CancellationToken cancellationToken)
    {
        var patient = await registerPatient.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PatientDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await getPatient.HandleAsync(id, cancellationToken);
        return patient is null ? NotFound() : patient;
    }

    [HttpGet("{id:guid}/cases")]
    [ProducesResponseType<IReadOnlyList<CaseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CaseDto>>> GetCases(Guid id, CancellationToken cancellationToken)
    {
        if (!await patients.ExistsAsync(id, cancellationToken))
        {
            return NotFound();
        }

        var result = await cases.GetByPatientIdAsync(id, cancellationToken);
        return Ok(result.Select(c => c.ToDto()).ToList());
    }
}
