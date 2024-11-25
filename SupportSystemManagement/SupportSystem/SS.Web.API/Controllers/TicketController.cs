using MediatR;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;

namespace SS.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketCommand command)
        {
            await _mediator.Send(command);
            return Ok("Ticket created successfully");
        }
    }
}
