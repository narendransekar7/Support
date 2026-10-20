using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Queries;

namespace SS.Audit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>The newest audit entries across all tickets.</summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int take = 50)
        {
            var entries = await _mediator.Send(new GetAuditLogsQuery(null, take));
            return Ok(entries);
        }

        /// <summary>One ticket's history, newest first.</summary>
        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetByTicket(Guid ticketId, [FromQuery] int take = 100)
        {
            var entries = await _mediator.Send(new GetAuditLogsQuery(ticketId, take));
            return Ok(entries);
        }

        /// <summary>One user's history (e.g. UserCreated), newest first.</summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int take = 100)
        {
            var entries = await _mediator.Send(new GetAuditLogsQuery(null, take, userId));
            return Ok(entries);
        }
    }
}
