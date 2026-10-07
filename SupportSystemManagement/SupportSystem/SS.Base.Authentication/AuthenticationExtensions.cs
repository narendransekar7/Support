using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SS.Base.Authentication;

// Shared bearer-token validation for every service that accepts the SPA's access token
// (SS.Gateway.API, SS.User.API). Two sign-in options issue tokens, so both are accepted:
//   - Microsoft Entra ID (OpenID Connect, authorization code + PKCE in the SPA): RS256 tokens,
//     validated against the tenant's published signing keys - config section "AzureAd".
//   - SS.Auth.Server.API email/password login: HS256 tokens signed with "Jwt:SigningKey".
// "Bearer" is a policy scheme that forwards to one of the two by the token's issuer, so Ocelot routes
// (AuthenticationProviderKey "Bearer") and [Authorize] accept either token type.
public static class AuthenticationExtensions
{
    public const string EntraIdScheme = "EntraId";
    public const string LocalScheme = "SupportSystem";

    public static IServiceCollection AddSupportSystemAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var azureAd = configuration.GetSection("AzureAd");
        var instance = (azureAd["Instance"] ?? "https://login.microsoftonline.com/").TrimEnd('/');
        var entraClientId = azureAd["ClientId"];

        var signingKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrEmpty(signingKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured (env var Jwt__SigningKey).");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddPolicyScheme(JwtBearerDefaults.AuthenticationScheme, "Entra ID or Support System token", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var header = context.Request.Headers.Authorization.ToString();
                    var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header["Bearer ".Length..].Trim() : null;
                    var handler = new JsonWebTokenHandler();
                    if (token is not null && handler.CanReadToken(token))
                    {
                        // Entra v2 tokens: https://login.microsoftonline.com/<tenant>/v2.0, v1: https://sts.windows.net/<tenant>/
                        var issuer = handler.ReadJsonWebToken(token).Issuer ?? "";
                        if (issuer.StartsWith(instance, StringComparison.OrdinalIgnoreCase) ||
                            issuer.StartsWith("https://sts.windows.net/", StringComparison.OrdinalIgnoreCase))
                        {
                            return EntraIdScheme;
                        }
                    }
                    return LocalScheme;
                };
            })
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
            })
            .AddJwtBearer(LocalScheme, options =>
            {
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
}
