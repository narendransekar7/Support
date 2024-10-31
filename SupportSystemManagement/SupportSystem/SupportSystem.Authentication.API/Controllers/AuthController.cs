using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SupportSystem.Authentication.API.Controllers;

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
        var client = _httpClientFactory.CreateClient("UserAPI");
        client.DefaultRequestHeaders.Add("X-Api-Key", "1234567890ABCDEF");
        var response = await client.PostAsJsonAsync("/api/user/validate", model);
        
        if (!response.IsSuccessStatusCode)
            return Unauthorized("Invalid credentials.");
        
        //if (model.Email == "user@example.com" && model.Password == "password")
        //{
            var token = GenerateJwtToken(model.Email);
            return Ok(new { token });
       // }
    }

    private string GenerateJwtToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}