using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SS.Base.Application.Commands;
using SS.Base.Application.Commands.User.LogOut;
using SS.Base.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SS.Auth.Server.API.Controllers
{
    // Owns the email/password sign-in end to end: checks credentials and stores/rotates/revokes refresh tokens
    // through the SS.Base.Application handlers (same database as SS.User.API), then mints the JWT.
    // Both tokens are returned only as HttpOnly cookies (AuthCookies), never in the response body, so the SPA's
    // JavaScript can't read them.
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);
        // Matches the expiry LoginSuccessHandler / TokenRefreshHandler store with the refresh token.
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        private readonly string _jwtSecret;

        public AuthController(IMediator mediator, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            // Shared with SS.Gateway.API / SS.User.API (SS.Base.Authentication), which validate these tokens.
            var signingKey = configuration["Jwt:SigningKey"];
            _jwtSecret = !string.IsNullOrEmpty(signingKey) ? signingKey
                : throw new InvalidOperationException("Jwt:SigningKey is not configured (env var Jwt__SigningKey).");
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ValidateUserQuery query)
        {
            if (!AuthCookies.HasCsrfHeader(Request))
                return MissingCsrfHeader();

            var user = await _mediator.Send(query);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var role = user.Role.ToString();

            // Persist the refresh token for the user so refresh-token/logout can find it later.
            var refreshToken = Guid.NewGuid();
            await _mediator.Send(new LoginSuccessCommand { Token = refreshToken, UserId = user.UserId.ToString() });

            WriteSessionCookies(user.PrimaryEmail, user.UserId.ToString(), role, refreshToken);
            _logger.LogInformation("Issued JWT for user {UserId} with role {Role}", user.UserId, role);

            return Ok(new { message = "Login successful." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!AuthCookies.HasCsrfHeader(Request))
                return MissingCsrfHeader();

            // Revokes the refresh token; the JWT itself stays valid until it expires, but its cookie is deleted.
            if (TryReadRefreshToken(out var refreshToken))
            {
                await _mediator.Send(new LogOutCommand { RefreshToken = refreshToken });
            }
            AuthCookies.Delete(Response);
            return Ok(new { message = "Logout successful." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!AuthCookies.HasCsrfHeader(Request))
                return MissingCsrfHeader();

            if (!TryReadRefreshToken(out var refreshToken))
                return Unauthorized("No refresh token.");

            var result = await _mediator.Send(new TokenRefreshCommand { RefreshToken = refreshToken });
            if (!result.Success)
            {
                // The session can't be continued: drop the stale cookies so the browser stops sending them.
                AuthCookies.Delete(Response);
                return Unauthorized(result.ErrorMessage);
            }

            WriteSessionCookies(result.Email, result.UserId, result.Role, result.NewRefreshToken);
            return Ok(new { message = "Token refreshed." });
        }

        private bool TryReadRefreshToken(out Guid refreshToken)
        {
            refreshToken = Guid.Empty;
            return Request.Cookies.TryGetValue(AuthCookies.RefreshToken, out var value) && Guid.TryParse(value, out refreshToken);
        }

        private void WriteSessionCookies(string email, string userId, string role, Guid refreshToken)
        {
            var now = DateTimeOffset.UtcNow;
            var accessTokenExpires = now.Add(AccessTokenLifetime);
            var token = GenerateJwtToken(email, userId, role, accessTokenExpires.UtcDateTime);
            AuthCookies.Write(Response, token, accessTokenExpires, refreshToken, now.Add(RefreshTokenLifetime));
        }

        private IActionResult MissingCsrfHeader() =>
            BadRequest($"The {AuthCookies.CsrfHeaderName} header is required.");

        private string GenerateJwtToken(string email, string userid, string role, DateTime expires)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Email, email),
                    new Claim("UserId", userid),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
