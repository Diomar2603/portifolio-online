using System.Net.Http.Headers;
using System.Net.Http.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Portfolio.PublishDispatcher;

/// <summary>
/// Acionada pelo S3 quando a API grava snapshot.json. Roda fora da VPC (tem internet sem NAT)
/// e dispara o workflow "publish-site" no GitHub via repository_dispatch.
///
/// Variáveis: GITHUB_REPOSITORY ("usuario/repo"), GITHUB_TOKEN_PARAMETER (nome do SecureString no SSM),
/// GITHUB_EVENT_TYPE (padrão "publish-site").
/// </summary>
public class Function
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://api.github.com/"),
        DefaultRequestHeaders =
        {
            UserAgent = { new ProductInfoHeaderValue("portfolio-publish-dispatcher", "1.0") },
            Accept = { new MediaTypeWithQualityHeaderValue("application/vnd.github+json") },
        },
    };

    private static readonly IAmazonSimpleSystemsManagement Ssm = new AmazonSimpleSystemsManagementClient();
    private static string? _token;

    public async Task Handler(S3Event s3Event, ILambdaContext context)
    {
        var record = s3Event.Records?.FirstOrDefault();
        context.Logger.LogInformation($"Snapshot atualizado: s3://{record?.S3.Bucket.Name}/{record?.S3.Object.Key}");

        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
            ?? throw new InvalidOperationException("GITHUB_REPOSITORY não definida");
        var eventType = Environment.GetEnvironmentVariable("GITHUB_EVENT_TYPE") ?? "publish-site";

        using var request = new HttpRequestMessage(HttpMethod.Post, $"repos/{repository}/dispatches")
        {
            Content = JsonContent.Create(new
            {
                event_type = eventType,
                client_payload = new { bucket = record?.S3.Bucket.Name, key = record?.S3.Object.Key },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());

        using var response = await Http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"GitHub respondeu {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        context.Logger.LogInformation($"repository_dispatch '{eventType}' enviado para {repository}");
    }

    private static async Task<string> GetTokenAsync()
    {
        if (_token is not null) return _token;
        var name = Environment.GetEnvironmentVariable("GITHUB_TOKEN_PARAMETER")
            ?? throw new InvalidOperationException("GITHUB_TOKEN_PARAMETER não definida");
        var result = await Ssm.GetParameterAsync(new GetParameterRequest { Name = name, WithDecryption = true });
        return _token = result.Parameter.Value;
    }
}
