namespace Portfolio.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Itens exibidos em ordem definida pelo admin.</summary>
public interface IOrdered
{
    int Order { get; set; }
}
