using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Extensions;
using Portfolio.Application.Abstractions;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Api.Endpoints;

/// <summary>
/// Upload direto ao S3: (1) POST devolve URLs pré-assinadas → (2) front faz PUT no S3 → (3) POST /confirm.
/// </summary>
public static class MediaEndpoints
{
    public static RouteGroupBuilder MapMedia(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/media").WithTags("media");

        group.MapGet("/", async (string? search, bool? unused, PortfolioDbContext db, CancellationToken ct) =>
        {
            var query = db.Media.AsNoTracking().Where(m => m.Status == MediaStatus.Ready);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => EF.Functions.ILike(m.Alt, $"%{search}%"));
            if (unused == true)
                query = query.Where(m =>
                    !db.ProjectMedia.Any(pm => pm.MediaId == m.Id) &&
                    !db.Projects.Any(p => p.CoverMediaId == m.Id) &&
                    !db.Certifications.Any(c => c.BadgeMediaId == m.Id) &&
                    !db.Profiles.Any(p => p.AvatarMediaId == m.Id));
            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync(ct);
        });

        group.MapPost("/", async (CreateMediaRequest req, PortfolioDbContext db, IMediaStorage storage, CancellationToken ct) =>
        {
            // Deduplicação por hash: se já existe, reaproveita
            if (await db.Media.AsNoTracking().FirstOrDefaultAsync(m => m.Sha256 == req.Sha256, ct) is { } existing)
                return Results.Ok(new CreateMediaResponse(existing.Id, [], Duplicate: true));

            var media = new Media
            {
                Key = $"img/{DateTime.UtcNow:yyyy/MM}/{Guid.CreateVersion7():N}",
                Width = req.Width,
                Height = req.Height,
                Sha256 = req.Sha256,
                Alt = req.Alt.Trim(),
                Bytes = req.Variants.Sum(v => v.Bytes),
                Variants = req.Variants.Select(v => v.Width).Order().ToArray(),
            };
            var urls = storage.CreateUploadUrls(media.Key, req.Variants.Select(v => (v.Width, v.Bytes)));

            db.Media.Add(media);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new CreateMediaResponse(media.Id, urls.Select(u => new PresignedUploadDto(u.Width, u.Url)).ToList(), Duplicate: false));
        }).Validate<CreateMediaRequest>();

        group.MapPost("/{id:guid}/confirm", async (Guid id, PortfolioDbContext db, IMediaStorage storage, CancellationToken ct) =>
        {
            if (await db.Media.FindAsync([id], ct) is not { } media) return Results.NotFound();

            foreach (var width in media.Variants)
                if (!await storage.ExistsAsync(S3MediaStorage.ObjectKey(media.Key, width), ct))
                    return Results.Problem($"Variante {width}px não encontrada no S3.", statusCode: 409);

            media.MarkReady();
            await db.SaveChangesAsync(ct);
            return Results.Ok(media);
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateMediaRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Alt)) return Results.Problem("Texto alternativo é obrigatório.", statusCode: 400);
            if (await db.Media.FindAsync([id], ct) is not { } media) return Results.NotFound();
            media.Alt = req.Alt.Trim();
            await db.SaveChangesAsync(ct);
            return Results.Ok(media);
        });

        group.MapDelete("/{id:guid}", async (Guid id, PortfolioDbContext db, IMediaStorage storage, CancellationToken ct) =>
        {
            if (await db.Media.FindAsync([id], ct) is not { } media) return Results.NotFound();
            if (await IsReferencedAsync(db, id, ct))
                return Results.Problem("A imagem está em uso.", statusCode: 409);

            await storage.DeleteAsync(media.Key, media.Variants, ct);
            db.Media.Remove(media);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return api;
    }

    private static async Task<bool> IsReferencedAsync(PortfolioDbContext db, Guid mediaId, CancellationToken ct) =>
        await db.ProjectMedia.AnyAsync(pm => pm.MediaId == mediaId, ct) ||
        await db.Projects.AnyAsync(p => p.CoverMediaId == mediaId, ct) ||
        await db.Certifications.AnyAsync(c => c.BadgeMediaId == mediaId, ct) ||
        await db.Profiles.AnyAsync(p => p.AvatarMediaId == mediaId, ct);
}
