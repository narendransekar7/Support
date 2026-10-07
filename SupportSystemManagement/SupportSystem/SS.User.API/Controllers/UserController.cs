using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
using SS.Base.Application.Commands.User.LogOut;
using SS.Base.Application.Queries;
using SS.Base.Domain.Dto;
using SS.Base.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SS.User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Endpoint to validate credentials  
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateUserQuery query)
        {
            var response = await _mediator.Send(query);

            if (response is not null)
            {
                var userData = new
                {
                    UserId = response?.UserId,
                    PrimaryEmail = response?.PrimaryEmail,
                    Role = response?.Role.ToString(),
                    DisplayName = response?.DisplayName,
                    FirstName = response?.FirstName,
                    LastName = response?.LastName
                };
                return Ok(userData);
            }
            else
            {
                return Unauthorized("Invalid credentials.");
            }
        }
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] TokenRefreshCommand query)
        {
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return Unauthorized(result.ErrorMessage);
            }

            return Ok(result);
        }

        [HttpPost("saverefreshtoken")]
        public async Task<IActionResult> SaveRefreshToken([FromBody] LoginSuccessCommand query)
        {
            await _mediator.Send(query);
            return Ok("Token saved successfully");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogOutCommand query)
        {
            await _mediator.Send(query);
            return Ok("Logout successfully");
        }

        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            await _mediator.Send(command);
            return Ok("User created successfully");
        }

        // The signed-in user (Microsoft Entra ID or password login), matched to a Support System account by email.
        // Identity comes from the validated token, never from the request, so a caller can only read itself.
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            // Password-login tokens and Entra's optional "email" claim (needed for guest accounts) carry "email";
            // otherwise Entra's preferred_username is the UPN/sign-in name.
            var email = User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("preferred_username") ?? User.FindFirstValue("upn");
            if (string.IsNullOrEmpty(email))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "The access token has no email claim.");
            }

            SS.Base.Domain.Entities.User user;
            try
            {
                user = await _mediator.Send(new GetUserByEmailQuery(email));
            }
            catch (KeyNotFoundException)
            {
                // Email deliberately not logged (PII); the Entra object id identifies who tried (absent for password logins).
                _logger.LogWarning("Signed-in user {ObjectId} has no Support System account", User.FindFirstValue("oid"));
                return StatusCode(StatusCodes.Status403Forbidden, "Your account is not registered in the Support System.");
            }

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                Name = user.DisplayName
            });
        }

        [HttpGet("fetchuser/{id}")]
        public async Task<IActionResult> FetchUserByEmail(string id)
        {
            var user = await _mediator.Send(new GetUserByEmailQuery(id));

            if (user is null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                Name = user.DisplayName
            };
            return Ok(userDto);
        }


    }
}