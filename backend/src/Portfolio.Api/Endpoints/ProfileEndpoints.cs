using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Extensions;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfile(this RouteGroupBuilder api)
    {
        api.MapGet("/profile", async (PortfolioDbContext db, CancellationToken ct) =>
            await db.Profiles.AsNoTracking().FirstOrDefaultAsync(ct) is { } p ? Results.Ok(p) : Results.NoContent());

        api.MapPut("/profile", async (ProfileRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            var profile = await db.Profiles.FirstOrDefaultAsync(ct);
            if (profile is null) db.Profiles.Add(profile = new Profile());

            profile.Name = req.Name;
            profile.Headline = req.Headline;
            profile.Bio = req.Bio;
            profile.Links = req.Links ?? [];
            profile.AvatarMediaId = req.AvatarMediaId;
            await db.SaveChangesAsync(ct);
            return Results.Ok(profile);
        }).Validate<ProfileRequest>();

        return api;
    }
}
