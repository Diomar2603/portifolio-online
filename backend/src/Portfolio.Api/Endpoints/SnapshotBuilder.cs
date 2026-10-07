using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

/// <summary>Monta o conteúdo publicado consumido pelo build estático do site.</summary>
public static class SnapshotBuilder
{
    public static async Task<object> BuildAsync(PortfolioDbContext db, IConfiguration config, CancellationToken ct)
    {
        var projects = await db.Projects.AsNoTracking()
            .Where(p => p.Status == PublishStatus.Published)
            .Include(p => p.Repos).Include(p => p.Tags).Include(p => p.Gallery)
            .ToListAsync(ct);

        return new
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
                content = JsonDocument.Parse(p.Content).RootElement.Clone(),
                repos = p.Repos.OrderBy(r => r.Order).Select(r => new { r.Url, r.Language, r.Order }),
                tags = p.Tags.Select(t => t.Name),
                gallery = p.Gallery.OrderBy(g => g.Order).Select(g => new { g.MediaId, g.Order, g.Caption }),
            }),
            media = await db.Media.AsNoTracking().Where(m => m.Status == MediaStatus.Ready).ToListAsync(ct),
        };
    }
}
