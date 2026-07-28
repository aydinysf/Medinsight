using MedInsight.AIOrchestration.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

/// <summary>Hızır sohbeti (ADR-018) — vaka kapsamlı, senkron, guardrail'li.</summary>
[ApiController]
[Route("api/v1/cases/{caseId:guid}/hizir-chat")]
[Authorize]
public sealed class HizirChatController(
    SendHizirChatMessageHandler sendMessage,
    GetHizirChatMessagesQueryHandler getMessages) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HizirChatMessageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HizirChatMessageDto>>> GetMessages(Guid caseId, CancellationToken cancellationToken)
    {
        var result = await getMessages.HandleAsync(caseId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Senkron LLM çağrısı — yanıt Hızır mesajı olarak döner (2-5 sn sürebilir).</summary>
    [HttpPost]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("messages")]
    [ProducesResponseType<HizirChatMessageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HizirChatMessageDto>> Send(Guid caseId, SendHizirChatMessage command, CancellationToken cancellationToken)
    {
        var result = await sendMessage.HandleAsync(caseId, command, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
