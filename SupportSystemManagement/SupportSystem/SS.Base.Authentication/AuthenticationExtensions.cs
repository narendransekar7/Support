using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SS.Base.Authentication;

// Every session - email/password or Microsoft Entra ID - ends up with the same token: an HS256 JWT minted by
// SS.Auth.Server.API (claims email, UserId, role from the Users table) and kept in the HttpOnly ss_access cookie
// (AuthCookies). Microsoft sign-in only proves who the user is: the SPA sends its Entra access token once to
// SS.Auth.Server.API (POST /api/auth/entra), which validates it with AddEntraIdTokenValidation and swaps it for
// a Support System session. So the gateway and the APIs only ever see Support System tokens, and roles always
// come from the Support System's own data.
public static class AuthenticationExtensions
{
    public const string EntraIdScheme = "EntraId";

    // SS.Gateway.API, SS.User.API: the Support System token, as the default "Bearer" scheme - the name ocelot*.json
    // routes reference as AuthenticationProviderKey. [Authorize(Roles = "Admin")] works off its "role" claim.
    public static IServiceCollection AddSupportSystemAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrEmpty(signingKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured (env var Jwt__SigningKey).");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // The SPA doesn't hold this token: it comes from the HttpOnly ss_access cookie (see AuthCookies),
                // accepted only with the CSRF header. An Authorization header, if present, is used as before.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Request.Headers.Authorization) &&
                            AuthCookies.HasCsrfHeader(context.Request) &&
                            context.Request.Cookies.TryGetValue(AuthCookies.AccessToken, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.MapInboundClaims = false; // claims as SS.Auth.Server.API writes them: email, UserId, role
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();
        return services;
    }

    // SS.Auth.Server.API only: validates the Microsoft Entra ID access token the SPA exchanges at /api/auth/entra
    // (RS256, checked against the tenant's published signing keys - config section "AzureAd"). Not a default
    // scheme: only endpoints with [Authorize(AuthenticationSchemes = EntraIdScheme)] use it.
    public static IServiceCollection AddEntraIdTokenValidation(this IServiceCollection services, IConfiguration configuration)
    {
        var azureAd = configuration.GetSection("AzureAd");
        var instance = (azureAd["Instance"] ?? "https://login.microsoftonline.com/").TrimEnd('/');
        var entraClientId = azureAd["ClientId"];

        services.AddAuthentication()
            .AddJwtBearer(EntraIdScheme, options =>
            {
                options.Authority = $"{instance}/{azureAd["TenantId"]}/v2.0";
                options.MapInboundClaims = false; // keep Entra claim names (oid, preferred_username, email, scp)
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    // v2 tokens carry the API's client id as aud, v1 tokens its App ID URI.
                    ValidAudiences = new[] { entraClientId, azureAd["Audience"] ?? $"api://{entraClientId}" },
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddAuthorization();
        return services;
    }
}
