using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
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

        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            await _mediator.Send(command);
            return Ok("User created successfully");
        }

        // The signed-in user (either sign-in option), matched to a Support System account by email.
        // Identity comes from the validated token, never from the request, so a caller can only read itself.
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            // SS.Auth.Server.API writes the account's email into every Support System token.
            var email = User.FindFirstValue("email");
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
                // Email deliberately not logged (PII); the token's UserId identifies who tried (e.g. a since-deleted account).
                _logger.LogWarning("Signed-in user {UserId} has no Support System account", User.FindFirstValue("UserId"));
                return StatusCode(StatusCodes.Status403Forbidden, "Your account is not registered in the Support System.");
            }

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Role = user.Role,
                Name = user.DisplayName
            });
        }

        // Admin screen (React /user/list): every account's id, name, email and role.
        [Authorize(Roles = "Admin")]
        [HttpGet("getallusers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            return Ok(users);
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