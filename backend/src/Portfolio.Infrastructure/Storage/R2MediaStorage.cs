using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Portfolio.Application.Abstractions;

namespace Portfolio.Infrastructure.Storage;

/// <summary>Cloudflare R2 via API compatível com S3.</summary>
public sealed class R2MediaStorage(IAmazonS3 s3, IOptions<R2Options> options) : IMediaStorage
{
    private readonly R2Options _opt = options.Value;

    public static string ObjectKey(string key, int width) => $"{key}-{width}.webp";

    public IReadOnlyList<PresignedUpload> CreateUploadUrls(string key, IEnumerable<(int Width, long Bytes)> variants) =>
        variants.Select(v =>
        {
            if (v.Bytes <= 0 || v.Bytes > _opt.MaxBytesPerVariant)
                throw new ArgumentOutOfRangeException(nameof(variants), $"Variante {v.Width}px excede o limite.");

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _opt.Bucket,
                Key = ObjectKey(key, v.Width),
                Verb = HttpVerb.PUT,
                ContentType = "image/webp",
                Expires = DateTime.UtcNow.AddMinutes(_opt.UploadUrlTtlMinutes),
            };
            return new PresignedUpload(v.Width, s3.GetPreSignedURL(request));
        }).ToList();

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            await s3.GetObjectMetadataAsync(_opt.Bucket, objectKey, ct);
            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, IEnumerable<int> widths, CancellationToken ct = default)
    {
        var objects = widths.Select(w => new KeyVersion { Key = ObjectKey(key, w) }).ToList();
        if (objects.Count > 0)
            await s3.DeleteObjectsAsync(new DeleteObjectsRequest { BucketName = _opt.Bucket, Objects = objects }, ct);
    }
}
