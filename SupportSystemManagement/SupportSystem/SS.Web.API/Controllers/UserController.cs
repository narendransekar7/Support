using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SS.Web.API.Controllers
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
        //[AllowAnonymous]
        //[HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateUserQuery query)
        {
            // code ned to be improved with invalid user, invalid credtial by returning from the API
            await _mediator.Send(query);
            return Ok("User validated successfully");
            //var user = await _userDatabasecontext.SS_User.SingleOrDefaultAsync(u => u.PrimaryEmail == loginDto.Email);
            //if (user == null)
            //    return Unauthorized("Invalid credentials.");

            //var passwordCheck = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            //if (passwordCheck == PasswordVerificationResult.Success)
            //    return Ok(new { isValid = true });
            //else
            //    return Unauthorized("Invalid credentials.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            await _mediator.Send(command);
            return Ok("User created successfully");
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    var users = await _mediator.Send(new GetAllTicketsQuery());
        //    return Ok(users);
        //}

    }
}
