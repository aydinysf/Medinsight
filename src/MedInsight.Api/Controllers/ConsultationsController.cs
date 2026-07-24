using MedInsight.Api.Hubs;
using MedInsight.Application.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/cases/{caseId:guid}/consultations")]
[Authorize]
public sealed class ConsultationsController(
    StartConsultationHandler startConsultation,
    GetCaseConsultationsQueryHandler getConsultations,
    SendConsultationMessageHandler sendMessage,
    GetConsultationMessagesQueryHandler getMessages,
    AddClinicalNoteHandler addClinicalNote,
    CompleteConsultationHandler completeConsultation,
    CreateTreatmentPlanHandler createTreatmentPlan,
    IHubContext<ConsultationHub> hub) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Patient,Caregiver,Admin")]
    [ProducesResponseType<ConsultationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConsultationDto>> Start(Guid caseId, StartConsultation command, CancellationToken cancellationToken)
    {
        var result = await startConsultation.HandleAsync(caseId, command, cancellationToken);
        return result is null ? NotFound() : StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ConsultationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConsultationDto>>> GetAll(Guid caseId, CancellationToken cancellationToken)
    {
        var result = await getConsultations.HandleAsync(caseId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{consultationId:guid}/messages")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("messages")]
    [ProducesResponseType<ConsultationMessageDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationMessageDto>> SendMessage(Guid caseId, Guid consultationId, SendConsultationMessage command, CancellationToken cancellationToken)
    {
        var message = await sendMessage.HandleAsync(caseId, consultationId, command, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        // Canlı yayın — içerik yalnızca gruba (vaka üyeleri) gider.
        await hub.Clients.Group(ConsultationHub.GroupName(consultationId))
            .SendAsync("messageReceived", message, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, message);
    }

    [HttpGet("{consultationId:guid}/messages")]
    [ProducesResponseType<IReadOnlyList<ConsultationMessageDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConsultationMessageDto>>> GetMessages(Guid caseId, Guid consultationId, CancellationToken cancellationToken)
    {
        var result = await getMessages.HandleAsync(caseId, consultationId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{consultationId:guid}/clinical-notes")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> AddNote(Guid caseId, Guid consultationId, AddClinicalNote command, CancellationToken cancellationToken)
    {
        var noteId = await addClinicalNote.HandleAsync(caseId, consultationId, command, cancellationToken);
        return noteId is null ? NotFound() : StatusCode(StatusCodes.Status201Created, new { noteId });
    }

    [HttpPost("{consultationId:guid}/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType<ConsultationDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationDto>> Complete(Guid caseId, Guid consultationId, CancellationToken cancellationToken)
    {
        var result = await completeConsultation.HandleAsync(caseId, consultationId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Tedavi planı — zorunlu HealthRoute snapshot'ı tetikler (invariant 2).</summary>
    [HttpPost("{consultationId:guid}/treatment-plan")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType<TreatmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TreatmentDto>> CreateTreatmentPlan(Guid caseId, Guid consultationId, CreateTreatmentPlan command, CancellationToken cancellationToken)
    {
        var result = await createTreatmentPlan.HandleAsync(caseId, consultationId, command, cancellationToken);
        return result is null ? NotFound() : StatusCode(StatusCodes.Status201Created, result);
    }
}
