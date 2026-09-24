using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

/// <summary>Árvore de categorias (lista de adjacência via ParentId).</summary>
public class Category : Entity, IOrdered
{
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }

    /// <summary>Impede ciclos: a categoria não pode ser movida para dentro de si mesma nem de um descendente.</summary>
    public bool CanMoveUnder(Guid? newParentId, IReadOnlyDictionary<Guid, Guid?> parentsById)
    {
        var current = newParentId;
        while (current is { } id)
        {
            if (id == Id) return false;
            current = parentsById.GetValueOrDefault(id);
        }
        return true;
    }
}
