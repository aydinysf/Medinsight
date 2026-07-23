using MedInsight.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace MedInsight.Infrastructure.Storage;

public sealed class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private volatile bool _bucketEnsured;

    public MinioObjectStorage(IConfiguration configuration)
    {
        var endpoint = configuration["Storage:Endpoint"]
            ?? throw new InvalidOperationException("'Storage:Endpoint' yapılandırılmamış.");

        _bucket = configuration["Storage:Bucket"] ?? "medinsight-documents";
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(configuration["Storage:AccessKey"], configuration["Storage:SecretKey"])
            .WithSSL(configuration.GetValue("Storage:UseSsl", false))
            .Build();
    }

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(cancellationToken);

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(key)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType),
            cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(cancellationToken);

        using var buffer = new MemoryStream();
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(key)
                .WithCallbackStream(stream => stream.CopyTo(buffer)),
            cancellationToken);

        return buffer.ToArray();
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (_bucketEnsured)
        {
            return;
        }

        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), cancellationToken);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), cancellationToken);
        }

        _bucketEnsured = true;
    }
}
