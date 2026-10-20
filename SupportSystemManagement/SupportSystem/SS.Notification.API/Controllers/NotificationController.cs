using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SS.Base.Application.Commands;
using SS.Base.Application.Queries;

namespace SS.Notification.API.Controllers
{
    /// <summary>
    /// The signed-in user's own notifications (the SPA's notification bell). The user comes from the
    /// validated Support System token's UserId claim, never from the request, so a caller can only see
    /// and change their own notifications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Unread count + the newest notifications.</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMine([FromQuery] int take = 20)
        {
            if (!TryGetUserId(out var userId))
                return Forbid();

            var feed = await _mediator.Send(new GetNotificationsByUserQuery(userId, take));
            return Ok(feed);
        }

        [HttpPost("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid notificationId)
        {
            if (!TryGetUserId(out var userId))
                return Forbid();

            var found = await _mediator.Send(new MarkNotificationReadCommand { UserId = userId, NotificationId = notificationId });
            return found ? NoContent() : NotFound();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            if (!TryGetUserId(out var userId))
                return Forbid();

            var changed = await _mediator.Send(new MarkAllNotificationsReadCommand { UserId = userId });
            return Ok(new { changed });
        }

        // SS.Auth.Server.API writes the account's UserId into every Support System token.
        private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirst("UserId")?.Value, out userId);
    }
}
