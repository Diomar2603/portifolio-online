using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Portfolio.Api.Auth;

public sealed class AdminAuthOptions
{
    public const string Section = "Auth";
    /// <summary>Grupo do Cognito exigido (claim cognito:groups).</summary>
    public string AdminGroup { get; set; } = "admin";
    /// <summary>Somente em desenvolvimento local: libera as rotas admin sem login.</summary>
    public bool DevBypass { get; set; }
}

/// <summary>
/// O token do Cognito é validado pelo JWT authorizer do API Gateway (assinatura, emissor,
/// audiência e expiração) antes de a Lambda ser invocada. O Amazon.Lambda.AspNetCoreServer
/// repassa as claims validadas em HttpContext.User; este handler só as reconhece e devolve
/// 401/403 quando faltam. A Lambda não tem URL pública: só o API Gateway pode invocá-la.
/// </summary>
public sealed class ApiGatewayAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<AdminAuthOptions> adminOptions,
    IHostEnvironment env)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiGateway";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.User.Identity?.IsAuthenticated == true)
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Context.User, SchemeName)));

        if (env.IsDevelopment() && adminOptions.Value.DevBypass)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", "dev"),
                new Claim("email", "dev@localhost"),
                new Claim("cognito:groups", adminOptions.Value.AdminGroup),
            }, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}

public static class ApiGatewayAuthentication
{
    public const string AdminPolicy = "admin";

    public static IServiceCollection AddApiGatewayAuth(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AdminAuthOptions>(config.GetSection(AdminAuthOptions.Section));
        var adminGroup = config.GetSection(AdminAuthOptions.Section).Get<AdminAuthOptions>()?.AdminGroup ?? "admin";

        services
            .AddAuthentication(ApiGatewayAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiGatewayAuthenticationHandler>(ApiGatewayAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, p => p
                .RequireAuthenticatedUser()
                // O API Gateway entrega cognito:groups como "[admin outro]" ou "admin"
                .RequireAssertion(ctx => ctx.User.FindAll("cognito:groups")
                    .SelectMany(c => c.Value.Trim('[', ']').Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    .Contains(adminGroup, StringComparer.Ordinal)));

        return services;
    }
}
