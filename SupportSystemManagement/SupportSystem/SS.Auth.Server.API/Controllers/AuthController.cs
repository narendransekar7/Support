using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SS.Base.Application.Commands;
using SS.Base.Application.Commands.User.LogOut;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SS.Auth.Server.API.Controllers
{
    // Owns the email/password sign-in end to end: checks credentials and stores/rotates/revokes refresh tokens
    // through the SS.Base.Application handlers (same database as SS.User.API), then mints the JWT.
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
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
            var user = await _mediator.Send(query);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var role = user.Role.ToString();
            var token = GenerateJwtToken(user.PrimaryEmail, user.UserId.ToString(), role);

            // Persist the refresh token for the user so refresh-token/logout can find it later.
            var refreshToken = Guid.NewGuid();
            await _mediator.Send(new LoginSuccessCommand { Token = refreshToken, UserId = user.UserId.ToString() });

            _logger.LogInformation("Issued JWT for user {UserId} with role {Role}", user.UserId, role);

            return Ok(new { token, refreshToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogOutCommand command)
        {
            // Revokes the refresh token; the JWT itself stays valid until it expires.
            await _mediator.Send(command);
            return Ok(new { message = "Logout successful." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return Unauthorized(result.ErrorMessage);

            var token = GenerateJwtToken(result.Email, result.UserId, result.Role);

            return Ok(new { token, refreshToken = result.NewRefreshToken });
        }

        private string GenerateJwtToken(string email, string userid, string role)
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
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
