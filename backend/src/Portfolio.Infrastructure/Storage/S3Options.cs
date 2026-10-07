namespace Portfolio.Infrastructure.Storage;

public sealed class S3Options
{
    public const string Section = "S3";
    /// <summary>Bucket das imagens, servido por CloudFront em media.seudominio.dev.</summary>
    public string MediaBucket { get; set; } = string.Empty;
    /// <summary>Bucket privado onde o Publicar grava snapshot.json.</summary>
    public string SnapshotBucket { get; set; } = string.Empty;
    public string SnapshotKey { get; set; } = "snapshot.json";
    public int UploadUrlTtlMinutes { get; set; } = 5;
    public long MaxBytesPerVariant { get; set; } = 5 * 1024 * 1024;
}
