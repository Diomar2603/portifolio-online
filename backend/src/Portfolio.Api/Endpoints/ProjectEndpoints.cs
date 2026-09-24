using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Extensions;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapProjects(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/projects").WithTags("projects");

        group.MapGet("/", async (PortfolioDbContext db, CancellationToken ct) =>
            await db.Projects.AsNoTracking().Include(p => p.Tags).OrderBy(p => p.Title).ToListAsync(ct));

        group.MapGet("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
            await Load(db).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct) is { } p ? Results.Ok(p) : Results.NotFound());

        group.MapPost("/", async (ProjectRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            var project = new Project();
            await ApplyAsync(project, req, db, ct);
            db.Projects.Add(project);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/projects/{project.Id}", project);
        }).Validate<ProjectRequest>();

        group.MapPut("/{id:guid}", async (Guid id, ProjectRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            if (await Load(db).FirstOrDefaultAsync(p => p.Id == id, ct) is not { } project) return Results.NotFound();
            await ApplyAsync(project, req, db, ct);
            await db.SaveChangesAsync(ct);
            return Results.Ok(project);
        }).Validate<ProjectRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
            await db.Projects.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0 ? Results.NoContent() : Results.NotFound());

        return api;
    }

    private static IQueryable<Project> Load(PortfolioDbContext db) =>
        db.Projects.Include(p => p.Repos).Include(p => p.Tags).Include(p => p.Gallery);

    private static async Task ApplyAsync(Project p, ProjectRequest r, PortfolioDbContext db, CancellationToken ct)
    {
        p.CategoryId = r.CategoryId;
        p.Title = r.Title;
        p.Slug = r.Slug;
        p.Summary = r.Summary;
        p.Content = r.Content;
        p.Featured = r.Featured;
        p.Status = r.Status == "published" ? PublishStatus.Published : PublishStatus.Draft;
        p.CoverMediaId = r.CoverMediaId;

        p.Repos.Clear();
        p.Repos.AddRange(r.Repos.Select(x => new ProjectRepo { Url = x.Url, Language = x.Language, Order = x.Order }));

        p.Gallery.Clear();
        p.Gallery.AddRange(r.Gallery.Select(x => new ProjectMedia { MediaId = x.MediaId, Order = x.Order, Caption = x.Caption }));

        // Tags: reaproveita existentes pelo slug e cria as novas
        var slugs = r.Tags.Select(Slug.From).Where(s => s.Length > 0).Distinct().ToList();
        var existing = await db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync(ct);
        p.Tags.Clear();
        foreach (var name in r.Tags.DistinctBy(Slug.From))
        {
            var slug = Slug.From(name);
            if (slug.Length == 0) continue;
            p.Tags.Add(existing.FirstOrDefault(t => t.Slug == slug) ?? new Tag { Name = name.Trim(), Slug = slug });
        }
    }
}
