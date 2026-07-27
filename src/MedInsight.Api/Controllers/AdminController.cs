using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Admin;
using MedInsight.Application.Doctors;
using MedInsight.Infrastructure.Audit;
using MedInsight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(
    ListPendingVerificationsQueryHandler listPending,
    ApproveVerificationHandler approve,
    RejectVerificationHandler reject,
    GetVerificationDocumentQueryHandler getDocument,
    IIdempotencyStore idempotency,
    MedInsightDbContext db) : ControllerBase
{
    /// <summary>Doğrulama belgesini stream eder — inceleme için (inline).</summary>
    [HttpGet("doctor-verifications/{id:guid}/document")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var result = await getDocument.HandleAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        Response.Headers.ContentDisposition = $"inline; filename=\"{result.FileName}\"";
        return File(result.Content, result.ContentType);
    }

    /// <summary>KVKK denetim sorgusu — yalnızca Admin rolü (audit-service.md).</summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLog>>> GetAuditLogs(
        [FromQuery] Guid? entityId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.AuditLogs.AsNoTracking().OrderByDescending(a => a.OccurredAtUtc).AsQueryable();
        if (entityId is not null)
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        return Ok(await query.Take(Math.Clamp(take, 1, 200)).ToListAsync(cancellationToken));
    }

    [HttpGet("doctor-verifications")]
    [ProducesResponseType<IReadOnlyList<PendingVerificationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PendingVerificationDto>>> GetPending(CancellationToken cancellationToken) =>
        Ok(await listPending.HandleAsync(cancellationToken));

    /// <summary>Idempotency-Key zorunlu — audit bütünlüğü (bkz. rate-limiting-idempotency.md).</summary>
    [HttpPost("doctor-verifications/{id:guid}/approve")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("admin-approve")]
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
