namespace Portfolio.Application.Abstractions;

public sealed record PresignedUpload(int Width, string Url);

/// <summary>Armazenamento de objetos (Cloudflare R2).</summary>
public interface IMediaStorage
{
    /// <summary>Gera URLs pré-assinadas de PUT para cada variante (expiram em poucos minutos).</summary>
    IReadOnlyList<PresignedUpload> CreateUploadUrls(string key, IEnumerable<(int Width, long Bytes)> variants);

    Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default);

    Task DeleteAsync(string key, IEnumerable<int> widths, CancellationToken ct = default);
}
