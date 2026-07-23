using System.Security.Cryptography;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Application.Abstractions.Repositories;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Common;
using MedInsight.Domain.Cases;
using MedInsight.Domain.Identity;

namespace MedInsight.Application.Documents;

public sealed record UploadFileInput(string FileName, string ContentType, byte[] Content);

public sealed record UploadDocuments(Guid CaseId, IReadOnlyList<UploadFileInput> Files);

public sealed record UploadedDocumentDto(Guid DocumentId, string FileName, DocumentStatus Status);

public sealed record UploadDocumentsResultDto(Guid CaseId, IReadOnlyList<UploadedDocumentDto> Documents);

public sealed class UploadDocumentsHandler(ICaseRepository cases, IObjectStorage storage, ICurrentUser currentUser)
{
    /// <summary>Vaka yoksa null döner. Dosyalar depoya yazılır, işleme (sınıflandırma/kalite) arka planda sürer → 202.</summary>
    public async Task<UploadDocumentsResultDto?> HandleAsync(UploadDocuments command, CancellationToken cancellationToken = default)
    {
        if (command.Files.Count == 0)
        {
            throw new ArgumentException("En az bir dosya gerekli.");
        }

        var medicalCase = await cases.GetByIdAsync(command.CaseId, cancellationToken);
        if (medicalCase is null)
        {
            return null;
        }

        EnsureCanUpload(medicalCase);

        var results = new List<UploadedDocumentDto>();
        foreach (var file in command.Files)
        {
            var contentHash = Convert.ToHexString(SHA256.HashData(file.Content)).ToLowerInvariant();
            var safeName = Path.GetFileName(file.FileName);
            var storageKey = $"cases/{medicalCase.Id}/{Guid.NewGuid():N}/{safeName}";

            using var stream = new MemoryStream(file.Content, writable: false);
            await storage.UploadAsync(storageKey, stream, file.ContentType, cancellationToken);

            var document = medicalCase.AddDocument(
                title: safeName,
                uploadedByUserId: currentUser.UserId,
                storageKey: storageKey,
                originalFileName: safeName,
                contentType: file.ContentType,
                sizeBytes: file.Content.LongLength,
                contentHash: contentHash);

            results.Add(new UploadedDocumentDto(document.Id, safeName, document.Status));
        }

        await cases.SaveChangesAsync(cancellationToken);
        return new UploadDocumentsResultDto(medicalCase.Id, results);
    }

    private void EnsureCanUpload(Case medicalCase)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return;
        }

        var member = medicalCase.Members.FirstOrDefault(m => m.UserId == currentUser.UserId);
        if (member is null || member.PermissionLevel < PermissionLevel.Contribute)
        {
            throw new ForbiddenAccessException("Bu vakaya belge yükleme yetkiniz yok.");
        }
    }
}
