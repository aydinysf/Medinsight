using MedInsight.Application.Cases;
using MedInsight.Application.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/patients")]
[Authorize]
public sealed class PatientsController(
    RegisterPatientHandler registerPatient,
    GetPatientQueryHandler getPatient,
    GetPatientCasesQueryHandler getPatientCases) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        var result = await getPatientCases.HandleAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
