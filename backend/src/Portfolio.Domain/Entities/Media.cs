using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

public enum MediaStatus { Pending, Ready }

public class Media : Entity
{
    /// <summary>Prefixo no R2; variantes ficam em {Key}-{largura}.webp.</summary>
    public string Key { get; set; } = string.Empty;
    public string Mime { get; set; } = "image/webp";
    public int Width { get; set; }
    public int Height { get; set; }
    public long Bytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    /// <summary>Texto alternativo obrigatório (acessibilidade).</summary>
    public string Alt { get; set; } = string.Empty;
    public MediaStatus Status { get; set; } = MediaStatus.Pending;
    public int[] Variants { get; set; } = [];

    public void MarkReady()
    {
        if (string.IsNullOrWhiteSpace(Alt))
            throw new InvalidOperationException("Texto alternativo é obrigatório.");
        Status = MediaStatus.Ready;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
