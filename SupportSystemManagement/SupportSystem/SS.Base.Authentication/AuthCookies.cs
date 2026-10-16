using Microsoft.AspNetCore.Http;

namespace SS.Base.Authentication;

// The email/password login keeps both of its tokens in HttpOnly cookies, so no JavaScript (including an injected
// script) can read them; the browser attaches them by itself. SS.Auth.Server.API writes them, the services using
// AddSupportSystemAuthentication read the access token from them.
//
// CSRF: because the browser sends cookies automatically, a cookie is only accepted together with the
// X-Requested-With header. A page on another site can't add custom headers to a cross-site request (and
// SameSite=Strict already keeps the browser from attaching the cookies to one).
public static class AuthCookies
{
    public const string AccessToken = "ss_access";
    public const string RefreshToken = "ss_refresh";

    public const string CsrfHeaderName = "X-Requested-With";
    public const string CsrfHeaderValue = "XMLHttpRequest";

    // Access token goes to every API call; the refresh token only to the Auth Server's own endpoints.
    private const string AccessTokenPath = "/api";
    private const string RefreshTokenPath = "/api/auth";

    public static bool HasCsrfHeader(HttpRequest request) =>
        string.Equals(request.Headers[CsrfHeaderName], CsrfHeaderValue, StringComparison.Ordinal);

    public static void Write(HttpResponse response, string accessToken, DateTimeOffset accessTokenExpires,
        Guid refreshToken, DateTimeOffset refreshTokenExpires)
    {
        response.Cookies.Append(AccessToken, accessToken, Options(AccessTokenPath, accessTokenExpires));
        response.Cookies.Append(RefreshToken, refreshToken.ToString(), Options(RefreshTokenPath, refreshTokenExpires));
    }

    public static void Delete(HttpResponse response)
    {
        response.Cookies.Delete(AccessToken, Options(AccessTokenPath, null));
        response.Cookies.Delete(RefreshToken, Options(RefreshTokenPath, null));
    }

    // Secure: browsers still accept Secure cookies on http://localhost (npm start, docker-compose); everywhere else
    // the app must be served over HTTPS anyway (see ARCHITECTURE.md "Authentication").
    private static CookieOptions Options(string path, DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = path,
        Expires = expires,
        IsEssential = true
    };
}
