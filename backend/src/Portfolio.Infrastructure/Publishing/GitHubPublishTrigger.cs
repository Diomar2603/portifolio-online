using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Portfolio.Application.Abstractions;

namespace Portfolio.Infrastructure.Publishing;

public sealed class GitHubOptions
{
    public const string Section = "GitHub";
    /// <summary>"usuario/repositorio"</summary>
    public string Repository { get; set; } = string.Empty;
    /// <summary>Fine-grained token com permissão Contents: write apenas neste repositório.</summary>
    public string Token { get; set; } = string.Empty;
    public string EventType { get; set; } = "publish-site";
}

/// <summary>POST /repos/{repo}/dispatches → workflow que prerenderiza e publica no Cloudflare Pages.</summary>
public sealed class GitHubPublishTrigger(HttpClient http, IOptions<GitHubOptions> options) : IPublishTrigger
{
    public async Task TriggerAsync(CancellationToken ct = default)
    {
        var opt = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"repos/{opt.Repository}/dispatches")
        {
            Content = JsonContent.Create(new { event_type = opt.EventType }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opt.Token);

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
