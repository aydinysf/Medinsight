using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Admin;
using MedInsight.Application.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(
    ListPendingVerificationsQueryHandler listPending,
    ApproveVerificationHandler approve,
    RejectVerificationHandler reject,
    IIdempotencyStore idempotency) : ControllerBase
{
    [HttpGet("doctor-verifications")]
    [ProducesResponseType<IReadOnlyList<PendingVerificationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PendingVerificationDto>>> GetPending(CancellationToken cancellationToken) =>
        Ok(await listPending.HandleAsync(cancellationToken));

    /// <summary>Idempotency-Key zorunlu — audit bütünlüğü (bkz. rate-limiting-idempotency.md).</summary>
    [HttpPost("doctor-verifications/{id:guid}/approve")]
    [ProducesResponseType<VerificationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerificationDto>> Approve(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest("Idempotency-Key başlığı zorunludur.");
        }

        var scopedKey = $"verification-approve:{id}:{idempotencyKey}";
        var stored = await idempotency.TryGetResponseAsync(scopedKey, cancellationToken);
        if (stored is not null)
        {
            return Ok(JsonSerializer.Deserialize<VerificationDto>(stored));
        }

        var result = await approve.HandleAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        await idempotency.SaveResponseAsync(scopedKey, JsonSerializer.Serialize(result), cancellationToken);
        return Ok(result);
    }

    public sealed record RejectRequest([Required] [StringLength(1000, MinimumLength = 3)] string Reason);

    [HttpPost("doctor-verifications/{id:guid}/reject")]
    [ProducesResponseType<VerificationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerificationDto>> Reject(Guid id, RejectRequest request, CancellationToken cancellationToken)
    {
        var result = await reject.HandleAsync(id, request.Reason, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
