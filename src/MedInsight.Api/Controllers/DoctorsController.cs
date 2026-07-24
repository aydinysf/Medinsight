using MedInsight.Application.Doctors;
using MedInsight.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/doctors")]
[Authorize]
public sealed class DoctorsController(
    RegisterDoctorHandler registerDoctor,
    SubmitVerificationHandler submitVerification,
    SetAvailabilityHandler setAvailability) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<DoctorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DoctorDto>> Register(RegisterDoctor command, CancellationToken cancellationToken)
    {
        // Kayıt serbest ama doğrulanana kadar doktor hiçbir vakaya erişemez (ADR-007).
        var doctor = await registerDoctor.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, doctor);
    }

    /// <summary>Diploma/uzmanlık belgesi + opsiyonel QR içeriği — admin onayına düşer.</summary>
    [HttpPost("me/verifications")]
    [Authorize(Roles = "Doctor")]
    [RequestSizeLimit(52_428_800)]
    [ProducesResponseType<VerificationDto>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerificationDto>> SubmitVerification(
        [FromForm] IFormFile document,
        [FromForm] VerificationDocumentType documentType,
        [FromForm] string? qrPayload,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await document.CopyToAsync(buffer, cancellationToken);

        var result = await submitVerification.HandleAsync(
            new SubmitVerification(documentType, document.FileName, document.ContentType, buffer.ToArray(), qrPayload),
            cancellationToken);

        return Accepted(result);
    }

    [HttpPut("me/availability")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType<AvailabilityDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AvailabilityDto>> SetAvailability(SetAvailability command, CancellationToken cancellationToken)
    {
        return await setAvailability.HandleAsync(command, cancellationToken);
    }
}
