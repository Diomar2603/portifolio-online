using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

/// <summary>Registro único com os dados do dono do portfólio.</summary>
public class Profile : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public Dictionary<string, string> Links { get; set; } = [];
    public Guid? AvatarMediaId { get; set; }
    public Media? AvatarMedia { get; set; }
}
