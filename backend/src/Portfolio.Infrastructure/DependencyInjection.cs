using System.Net.Http.Headers;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Abstractions;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Publishing;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PortfolioDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Postgres"), npgsql => npgsql.EnableRetryOnFailure()));

        services.Configure<R2Options>(config.GetSection(R2Options.Section));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var r2 = sp.GetRequiredService<IOptions<R2Options>>().Value;
            return new AmazonS3Client(r2.AccessKeyId, r2.SecretAccessKey, new AmazonS3Config
            {
                ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
                AuthenticationRegion = "auto",
                ForcePathStyle = true,
            });
        });
        services.AddSingleton<IMediaStorage, R2MediaStorage>();

        services.Configure<GitHubOptions>(config.GetSection(GitHubOptions.Section));
        services.AddHttpClient<IPublishTrigger, GitHubPublishTrigger>(c =>
        {
            c.BaseAddress = new Uri("https://api.github.com/");
            c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("portfolio-api", "1.0"));
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        });

        return services;
    }
}
