namespace Portfolio.Infrastructure.Storage;

public sealed class R2Options
{
    public const string Section = "R2";
    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "portfolio-media";
    public int UploadUrlTtlMinutes { get; set; } = 5;
    public long MaxBytesPerVariant { get; set; } = 5 * 1024 * 1024;
}
