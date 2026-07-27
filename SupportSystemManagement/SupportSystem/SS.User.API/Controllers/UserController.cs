using System.Security.Claims;
using MediatR;
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

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
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