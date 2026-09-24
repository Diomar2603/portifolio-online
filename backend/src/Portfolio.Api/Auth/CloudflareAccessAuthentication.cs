using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Portfolio.Api.Auth;

public sealed class CloudflareAccessOptions
{
    public const string Section = "CloudflareAccess";
    /// <summary>Ex.: "seutime" → https://seutime.cloudflareaccess.com</summary>
    public string TeamName { get; set; } = string.Empty;
    /// <summary>Application Audience (AUD) Tag do app no Zero Trust.</summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>E-mails autorizados (defesa extra além da política do Access).</summary>
    public string[] AllowedEmails { get; set; } = [];
}

/// <summary>
/// Valida o JWT que o Cloudflare Access injeta no header Cf-Access-Jwt-Assertion.
/// Requisições que chegam direto no domínio do Azure (sem passar pelo Access) são rejeitadas.
/// </summary>
public static class CloudflareAccessAuthentication
{
    public const string HeaderName = "Cf-Access-Jwt-Assertion";
    public const string AdminPolicy = "admin";

    public static IServiceCollection AddCloudflareAccess(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var cf = config.GetSection(CloudflareAccessOptions.Section).Get<CloudflareAccessOptions>() ?? new();

        // Desenvolvimento local sem Access configurado: libera as rotas admin.
        if (env.IsDevelopment() && string.IsNullOrEmpty(cf.TeamName))
        {
            services.AddAuthentication();
            services.AddAuthorizationBuilder().AddPolicy(AdminPolicy, p => p.RequireAssertion(_ => true));
            return services;
        }
        var issuer = $"https://{cf.TeamName}.cloudflareaccess.com";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                // As chaves públicas do Access ficam em /cdn-cgi/access/certs (JWKS).
                o.ConfigurationManager = new StaticJwksConfigurationManager(issuer);
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = cf.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Headers.TryGetValue(HeaderName, out var token))
                            ctx.Token = token.ToString();
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, p => p
                .RequireAuthenticatedUser()
                .RequireAssertion(ctx =>
                    cf.AllowedEmails.Length == 0 ||
                    ctx.User.FindFirst("email")?.Value is { } email &&
                    cf.AllowedEmails.Contains(email, StringComparer.OrdinalIgnoreCase)));

        return services;
    }

    /// <summary>Carrega o JWKS do Access e o reaproveita (com atualização periódica).</summary>
    private sealed class StaticJwksConfigurationManager(string issuer) : IConfigurationManager<OpenIdConnectConfiguration>
    {
        private static readonly HttpClient Http = new();
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
        private OpenIdConnectConfiguration? _config;
        private DateTimeOffset _loadedAt;

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            if (_config is not null && DateTimeOffset.UtcNow - _loadedAt < RefreshInterval) return _config;

            var json = await Http.GetStringAsync($"{issuer}/cdn-cgi/access/certs", cancel);
            var config = new OpenIdConnectConfiguration { Issuer = issuer };
            foreach (var key in new JsonWebKeySet(json).GetSigningKeys())
                config.SigningKeys.Add(key);

            _config = config;
            _loadedAt = DateTimeOffset.UtcNow;
            return config;
        }

        public void RequestRefresh() => _loadedAt = DateTimeOffset.MinValue;
    }
}
