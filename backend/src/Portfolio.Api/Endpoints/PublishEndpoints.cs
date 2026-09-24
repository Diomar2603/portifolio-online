using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Abstractions;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

public static class PublishEndpoints
{
    public static RouteGroupBuilder MapPublishing(this RouteGroupBuilder api)
    {
        // Dispara o workflow que prerenderiza o site e publica no Cloudflare Pages.
        api.MapPost("/publish", async (IPublishTrigger trigger, CancellationToken ct) =>
        {
            await trigger.TriggerAsync(ct);
            return Results.Accepted();
        }).WithTags("publishing");

        // Conteúdo publicado — consumido pelo build (scripts/fetch-snapshot.mjs no frontend).
        api.MapGet("/snapshot", async (PortfolioDbContext db, IConfiguration config, CancellationToken ct) =>
        {
            var projects = await db.Projects.AsNoTracking()
                .Where(p => p.Status == PublishStatus.Published)
                .Include(p => p.Repos).Include(p => p.Tags).Include(p => p.Gallery)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                generatedAt = DateTimeOffset.UtcNow,
                mediaBaseUrl = config["Site:MediaBaseUrl"],
                profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(ct),
                education = await db.Education.AsNoTracking().OrderBy(x => x.Order).ToListAsync(ct),
                experience = await db.Experience.AsNoTracking().OrderBy(x => x.Order).ToListAsync(ct),
                certifications = await db.Certifications.AsNoTracking().OrderBy(x => x.Order).ToListAsync(ct),
                categories = await db.Categories.AsNoTracking().OrderBy(x => x.Order).ToListAsync(ct),
                projects = projects.Select(p => new
                {
                    p.Id, p.CategoryId, p.Title, p.Slug, p.Summary, p.Featured, p.CoverMediaId,
                    status = "published",
                    content = System.Text.Json.JsonDocument.Parse(p.Content).RootElement,
                    repos = p.Repos.OrderBy(r => r.Order).Select(r => new { r.Url, r.Language, r.Order }),
                    tags = p.Tags.Select(t => t.Name),
                    gallery = p.Gallery.OrderBy(g => g.Order).Select(g => new { g.MediaId, g.Order, g.Caption }),
                }),
                media = await db.Media.AsNoTracking().Where(m => m.Status == MediaStatus.Ready).ToListAsync(ct),
            });
        }).WithTags("publishing");

        return api;
    }
}
