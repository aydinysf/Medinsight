using MedInsight.Application.Abstractions.Dicom;
using MedInsight.Application.Abstractions.Storage;
using MedInsight.Application.Abstractions.TextExtraction;

namespace MedInsight.Application.Tests.Fakes;

public sealed class FakeObjectStorage : IObjectStorage
{
    public Dictionary<string, byte[]> Objects { get; } = [];

    public Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        Objects[key] = buffer.ToArray();
        return Task.CompletedTask;
    }

    public Task<byte[]> DownloadAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Objects[key]);
}

public sealed class FakeDicomReader : IDicomMetadataReader
{
    public DicomFileMetadata? Metadata { get; set; }

    public DicomIntegrityInfo? Integrity { get; set; } = new(true, true, true);

    public DicomFileMetadata? Read(byte[] content) => Metadata;

    public DicomIntegrityInfo? ReadIntegrity(byte[] content) => Integrity;
}

public sealed class FakeOcrProvider(string name = "FakeOcr", string text = "", decimal confidence = 0) : IOcrProvider
{
    public string Name => name;

    public Task<OcrResult> ExtractTextAsync(byte[] content, CancellationToken cancellationToken = default) =>
        Task.FromResult(new OcrResult(text, confidence, Name));
}
