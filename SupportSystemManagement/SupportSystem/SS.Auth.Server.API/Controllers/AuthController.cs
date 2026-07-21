using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace SS.Auth.Server.API.Controllers
{
    [Route("api/auth")]
    //[Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _jwtSecret = "hldiSW6BAHCCzY9Yy1zQLiN+MHYJ0Fm5InfQlPANUyM=";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;


        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            client.DefaultRequestHeaders.Add("X-Api-Key", "1234567890ABCDEF");
            //dynamic user = await client.PostAsJsonAsync("/api/user/validate", model);

            //if (user==null)
            //    return Unauthorized("Invalid credentials.");

            // Call Validate API
            var response = await client.PostAsJsonAsync("/api/user/validate", model);

            if (!response.IsSuccessStatusCode)
            {
                // Forward status + message from validate API
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            // ✅ Deserialize JSON response into UserDto
            var user = await response.Content.ReadFromJsonAsync<UserDto>();

            if (user == null)
                return Unauthorized("Invalid credentials.");



            //if (model.Email == "user@example.com" && model.Password == "password")
            //{

            var token = GenerateJwtToken(model.Email, user.UserId.ToString(), user.Role);
            var refreshToken = GenerateRefreshToken();

            //Save refresh token with in the datbase for the user.
            var refreshTokenSaveResponse = await client.PostAsJsonAsync("/api/user/saverefreshtoken",  new RefreshTokenModel { Token = refreshToken,UserId = user.UserId.ToString() });

            return Ok(new { token,refreshToken,});
            // }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutModel model)
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            client.DefaultRequestHeaders.Add("X-Api-Key", "1234567890ABCDEF");

            // Call API to invalidate the refresh token
            var response = await client.PostAsJsonAsync("/api/user/logout", model);

            if (!response.IsSuccessStatusCode)
            {
                // Forward status + message from invalidate API
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            return Ok(new { message = "Logout successful." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] LogoutModel refreshTokenDto)
        {

            var client = _httpClientFactory.CreateClient("WebAPI");
            client.DefaultRequestHeaders.Add("X-Api-Key", "1234567890ABCDEF");

            // Call API to refresh access and  refresh token
            var response = await client.PostAsJsonAsync("/api/user/refresh-token", refreshTokenDto);

            if (!response.IsSuccessStatusCode)
            {
                // Forward status + message from invalidate API
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var result = await response.Content.ReadFromJsonAsync<TokenRefreshResultDto>();

            if (result == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var token = GenerateJwtToken(result.Email, result.UserId, result.Role);

            return Ok(new { token, refreshToken = result.NewRefreshToken });
        }


        private string GenerateJwtToken(string email,string userid,string role)
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
        
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString(); // Use a more secure random generator for production
        }



    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string PrimaryEmail { get; set; }
        public string Role { get; set; }
    }

    public class LogoutModel
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
    }

    public class TokenRefreshResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public Guid NewRefreshToken { get; set; }
    }
}
