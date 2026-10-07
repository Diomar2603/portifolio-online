using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Portfolio.Application.Abstractions;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Endpoints;

public static class PublishEndpoints
{
    public static RouteGroupBuilder MapPublishing(this RouteGroupBuilder api)
    {
        // Grava snapshot.json no S3 → Lambda PublishDispatcher → GitHub Actions (prerender + deploy no S3/CloudFront).
        api.MapPost("/publish", async (PortfolioDbContext db, IConfiguration config, IPublishTrigger trigger,
            IOptions<JsonOptions> json, CancellationToken ct) =>
        {
            var snapshot = await SnapshotBuilder.BuildAsync(db, config, ct);
            await trigger.PublishAsync(JsonSerializer.SerializeToUtf8Bytes(snapshot, json.Value.SerializerOptions), ct);
            return Results.Accepted();
        }).WithTags("publishing");

        // Pré-visualização do que será publicado (útil no desenvolvimento local).
        api.MapGet("/snapshot", async (PortfolioDbContext db, IConfiguration config, CancellationToken ct) =>
            Results.Ok(await SnapshotBuilder.BuildAsync(db, config, ct))).WithTags("publishing");

        return api;
    }
}
