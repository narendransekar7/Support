using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
using SS.Base.Application.Queries;
using SS.Base.Domain.Dto;
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
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateUserQuery query)
        {
            // code ned to be improved with invalid user, invalid credtial by returning from the API
            var response =await _mediator.Send(query);
            if(response)
            return Ok("User validated successfully");
            else
            {
                return Unauthorized("Invalid credentials.");
            }
            //var user = await _userDatabasecontext.SS_User.SingleOrDefaultAsync(u => u.PrimaryEmail == loginDto.Email);
            //if (user == null)
            //    return Unauthorized("Invalid credentials.");

            //var passwordCheck = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            //if (passwordCheck == PasswordVerificationResult.Success)
            //    return Ok(new { isValid = true });
            //else
            //    return Unauthorized("Invalid credentials.");
        }
        
        [HttpPost("saverefreshtoken")]
        public async Task<IActionResult> SaveRefreshToken([FromBody] LoginSuccessCommand query)
        {
            await _mediator.Send(query);
            return Ok("Token saved successfully");
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
            
            //var user = await _mediator.Send(new GetUserByIdQuery(ClaimTypes.Email));
            //if (user == null)
          //      return NotFound();
            
           var user = await _mediator.Send(new GetUserByEmailQuery(id));
          
           var userDto = new UserDto
           {
               UserId = user.UserId,
               Role = user.Role,
               Name = user.DisplayName
           };
            return Ok(userDto);
        }
        //[HttpGet]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    var users = await _mediator.Send(new GetAllTicketsQuery());
        //    return Ok(users);
        //}

    }
    

}
