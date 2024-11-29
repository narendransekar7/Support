using MediatR;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;

namespace SS.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketUpdateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketUpdateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddTicketUpdateCommand command)
        {
            await _mediator.Send(command);
            return Ok("Ticket updated successfully");
        }
    }
}
