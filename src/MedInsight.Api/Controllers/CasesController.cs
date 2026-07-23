using MedInsight.Application.Cases;
using MedInsight.TimelineService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/cases")]
[Authorize]
public sealed class CasesController(
    CreateCaseHandler createCase,
    GetCaseQueryHandler getCase,
    ITimelineStore timeline) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Patient,Admin")]
    [ProducesResponseType<CaseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDto>> Create(CreateCase command, CancellationToken cancellationToken)
    {
        var result = await createCase.HandleAsync(command, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CaseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var medicalCase = await getCase.HandleAsync(id, cancellationToken);
        return medicalCase is null ? NotFound() : medicalCase;
    }

    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType<IReadOnlyList<TimelineEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TimelineEntry>>> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        // Erişim kontrolü (üyelik/Admin) query handler'da — vaka yoksa 404.
        if (await getCase.HandleAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        return Ok(await timeline.GetByCaseAsync(id, cancellationToken));
    }
}
