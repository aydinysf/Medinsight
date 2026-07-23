using System.Text.Json;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Controllers;

[ApiController]
[Route("api/v1/cases/{caseId:guid}/documents")]
[Authorize]
public sealed class DocumentsController(
    UploadDocumentsHandler uploadDocuments,
    GetCaseDocumentsQueryHandler getDocuments,
    IIdempotencyStore idempotency) : ControllerBase
{
    /// <summary>
    /// Toplu belge yükleme. 202 döner: dosyalar kabul edildi, sınıflandırma ve
    /// kalite kontrolü arka planda sürer (bkz. docs/architecture/api-design.md).
    /// Idempotency-Key başlığı tekrarlanan isteği yeniden işlemez.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(1_073_741_824)]
    [RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824, ValueCountLimit = 2000)]
    [ProducesResponseType<UploadDocumentsResultDto>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UploadDocumentsResultDto>> Upload(
        Guid caseId,
        [FromForm] List<IFormFile> files,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var scopedKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : $"upload:{caseId}:{idempotencyKey}";
        if (scopedKey is not null)
        {
            var stored = await idempotency.TryGetResponseAsync(scopedKey, cancellationToken);
            if (stored is not null)
            {
                return Accepted(JsonSerializer.Deserialize<UploadDocumentsResultDto>(stored));
            }
        }

        var inputs = new List<UploadFileInput>(files.Count);
        foreach (var file in files)
        {
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer, cancellationToken);
            inputs.Add(new UploadFileInput(file.FileName, file.ContentType, buffer.ToArray()));
        }

        var result = await uploadDocuments.HandleAsync(new UploadDocuments(caseId, inputs), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        if (scopedKey is not null)
        {
            await idempotency.SaveResponseAsync(scopedKey, JsonSerializer.Serialize(result), cancellationToken);
        }

        return Accepted(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CaseDocumentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CaseDocumentDto>>> GetAll(Guid caseId, CancellationToken cancellationToken)
    {
        var result = await getDocuments.HandleAsync(caseId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
