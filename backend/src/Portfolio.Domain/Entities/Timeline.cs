using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

public class Education : Entity, IOrdered
{
    public string Institution { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}

public class Experience : Entity, IOrdered
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}

public class Certification : Entity, IOrdered
{
    public string Name { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateOnly IssuedAt { get; set; }
    public string? CredentialUrl { get; set; }
    public Guid? BadgeMediaId { get; set; }
    public Media? BadgeMedia { get; set; }
    public int Order { get; set; }
}
