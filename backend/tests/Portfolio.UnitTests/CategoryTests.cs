using Portfolio.Domain.Entities;

namespace Portfolio.UnitTests;

public class CategoryTests
{
    [Fact]
    public void Nao_permite_mover_categoria_para_dentro_de_um_descendente()
    {
        var raiz = new Category { Name = "Back-end" };
        var filho = new Category { Name = ".NET", ParentId = raiz.Id };
        var neto = new Category { Name = "APIs", ParentId = filho.Id };
        var parents = new Dictionary<Guid, Guid?>
        {
            [raiz.Id] = null,
            [filho.Id] = raiz.Id,
            [neto.Id] = filho.Id,
        };

        Assert.False(raiz.CanMoveUnder(neto.Id, parents));
        Assert.False(raiz.CanMoveUnder(raiz.Id, parents));
        Assert.True(neto.CanMoveUnder(null, parents));
    }
}

public class MediaTests
{
    [Fact]
    public void MarkReady_exige_texto_alternativo()
    {
        var media = new Media { Alt = " " };
        Assert.Throws<InvalidOperationException>(media.MarkReady);
    }
}
