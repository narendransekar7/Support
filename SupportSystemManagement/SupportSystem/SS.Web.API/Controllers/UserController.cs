using MediatR;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;

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
