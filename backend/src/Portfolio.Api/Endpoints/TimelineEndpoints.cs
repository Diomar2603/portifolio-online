using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Extensions;
using Portfolio.Application.Contracts;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

/// <summary>CRUD de Formação, Experiência e Certificações (mesmo formato, entidades diferentes).</summary>
public static class TimelineEndpoints
{
    public static RouteGroupBuilder MapTimeline(this RouteGroupBuilder api)
    {
        MapCrud<Education, EducationRequest>(api, "education", (e, r) =>
        {
            e.Institution = r.Institution; e.Course = r.Course; e.StartDate = r.StartDate;
            e.EndDate = r.EndDate; e.Description = r.Description; e.Order = r.Order;
        });

        MapCrud<Experience, ExperienceRequest>(api, "experience", (e, r) =>
        {
            e.Company = r.Company; e.Role = r.Role; e.StartDate = r.StartDate;
            e.EndDate = r.EndDate; e.Description = r.Description; e.Order = r.Order;
        });

        MapCrud<Certification, CertificationRequest>(api, "certifications", (e, r) =>
        {
            e.Name = r.Name; e.Issuer = r.Issuer; e.IssuedAt = r.IssuedAt;
            e.CredentialUrl = r.CredentialUrl; e.BadgeMediaId = r.BadgeMediaId; e.Order = r.Order;
        });

        return api;
    }

    private static void MapCrud<TEntity, TRequest>(RouteGroupBuilder api, string route, Action<TEntity, TRequest> apply)
        where TEntity : Entity, IOrdered, new()
        where TRequest : class
    {
        var group = api.MapGroup($"/{route}").WithTags(route);

        group.MapGet("/", async (PortfolioDbContext db, CancellationToken ct) =>
            await db.Set<TEntity>().AsNoTracking().OrderBy(x => x.Order).ToListAsync(ct));

        group.MapGet("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
            await db.Set<TEntity>().FindAsync([id], ct) is { } e ? Results.Ok(e) : Results.NotFound());

        group.MapPost("/", async (TRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            var entity = new TEntity();
            apply(entity, req);
            db.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/{route}/{entity.Id}", entity);
        }).Validate<TRequest>();

        group.MapPut("/{id:guid}", async (Guid id, TRequest req, PortfolioDbContext db, CancellationToken ct) =>
        {
            if (await db.Set<TEntity>().FindAsync([id], ct) is not { } entity) return Results.NotFound();
            apply(entity, req);
            await db.SaveChangesAsync(ct);
            return Results.Ok(entity);
        }).Validate<TRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, PortfolioDbContext db, CancellationToken ct) =>
            await db.Set<TEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct) > 0 ? Results.NoContent() : Results.NotFound());
    }
}
