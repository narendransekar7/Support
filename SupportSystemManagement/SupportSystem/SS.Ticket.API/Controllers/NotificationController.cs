using MediatR;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Queries;

namespace SS.Ticket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            var notifications = await _mediator.Send(new GetNotificationsByUserQuery(userId));
            return Ok(notifications);
        }
    }
}
