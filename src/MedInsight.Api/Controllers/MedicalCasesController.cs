using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.MedicalCases;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("patients/{patientId:guid}/cases")]
public sealed class MedicalCasesController(
    CreateMedicalCaseService createMedicalCase,
    IPatientRepository patients,
    IMedicalCaseRepository medicalCases) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MedicalCaseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalCaseDto>> Create(Guid patientId, MedicalCaseCreateDto dto, CancellationToken cancellationToken)
    {
        var medicalCase = await createMedicalCase.ExecuteAsync(patientId, dto, cancellationToken);
        if (medicalCase is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetByPatient), new { patientId }, medicalCase);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicalCaseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MedicalCaseDto>>> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        if (!await patients.ExistsAsync(patientId, cancellationToken))
        {
            return NotFound();
        }

        var cases = await medicalCases.GetByPatientIdAsync(patientId, cancellationToken);
        return Ok(cases.Select(c => c.ToDto()).ToList());
    }
}
