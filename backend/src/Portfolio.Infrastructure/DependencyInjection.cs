using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Abstractions;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Publishing;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Aurora Serverless v2 pode estar pausado (0 ACU): a primeira conexão leva ~15 s.
        services.AddDbContext<PortfolioDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Postgres"), npgsql => npgsql
                .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)
                .CommandTimeout(30)));

        services.Configure<S3Options>(config.GetSection(S3Options.Section));
        // Credenciais e região vêm do papel IAM da Lambda (ou do perfil local em desenvolvimento).
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client());
        services.AddSingleton<IMediaStorage, S3MediaStorage>();
        services.AddSingleton<IPublishTrigger, S3PublishTrigger>();

        return services;
    }
}
