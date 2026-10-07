using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Portfolio.Application.Abstractions;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure.Publishing;

/// <summary>
/// Grava snapshot.json no bucket privado. A notificação do S3 aciona a Lambda PublishDispatcher
/// (fora da VPC), que chama o GitHub. Assim a API, dentro da VPC, não precisa de NAT Gateway.
/// </summary>
public sealed class S3PublishTrigger(IAmazonS3 s3, IOptions<S3Options> options) : IPublishTrigger
{
    public async Task PublishAsync(byte[] snapshotJson, CancellationToken ct = default)
    {
        var opt = options.Value;
        using var body = new MemoryStream(snapshotJson);
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = opt.SnapshotBucket,
            Key = opt.SnapshotKey,
            InputStream = body,
            ContentType = "application/json",
        }, ct);
    }
}
