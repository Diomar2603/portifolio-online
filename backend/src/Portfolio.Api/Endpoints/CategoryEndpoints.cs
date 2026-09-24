using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Extensions;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategories(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/categories").WithTags("categories");

        // Lista plana ordenada; o front monta a árvore a partir de ParentId.
        group.MapGet("/", async (PortfolioDbContext db, CancellationToken ct) =>
            await db.Categories.AsNoTracking().OrderBy(c => c.ParentId).ThenBy(c => c.Order).ToListAsync(ct));

        group.MapPost("/", async (CategoryRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            var category = new Category();
            Apply(category, req);
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/categories/{category.Id}", category);
        }).Validate<CategoryRequest>();

        group.MapPut("/{id:guid}", async (Guid id, CategoryRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            if (await db.Categories.FindAsync([id], ct) is not { } category) return Results.NotFound();

            var parents = await db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.ParentId, ct);
            if (!category.CanMoveUnder(req.ParentId, parents))
                return Results.Problem("A categoria não pode ficar dentro dela mesma ou de uma subcategoria.", statusCode: 409);

            Apply(category, req);
            await db.SaveChangesAsync(ct);
            return Results.Ok(category);
        }).Validate<CategoryRequest>();

        group.MapPatch("/reorder", async (ReorderItem[] items, PortfolioDbContext db, CancellationToken ct) =>
        {
            var ids = items.Select(i => i.Id).ToHashSet();
            var categories = await db.Categories.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, ct);
            foreach (var item in items)
            {
                if (!categories.TryGetValue(item.Id, out var c)) continue;
                c.ParentId = item.ParentId;
                c.Order = item.Order;
            }
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
        {
            if (await db.Categories.AnyAsync(c => c.ParentId == id, ct))
                return Results.Problem("Remova ou mova as subcategorias antes.", statusCode: 409);
            if (await db.Projects.AnyAsync(p => p.CategoryId == id, ct))
                return Results.Problem("Existem projetos nesta categoria.", statusCode: 409);

            return await db.Categories.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0 ? Results.NoContent() : Results.NotFound();
        });

        return api;
    }

    private static void Apply(Category c, CategoryRequest r)
    {
        c.ParentId = r.ParentId; c.Name = r.Name; c.Slug = r.Slug;
        c.Order = r.Order; c.Icon = r.Icon; c.Color = r.Color;
    }
}
