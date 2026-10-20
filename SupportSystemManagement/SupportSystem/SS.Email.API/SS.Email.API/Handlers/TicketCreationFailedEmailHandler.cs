using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Handlers;

/// <summary>
/// Compensation email when the ticket-creation saga couldn't assign an engineer.
/// Called by a Kafka consumer worker or by TicketCreationFailedEmailConsumer (RabbitMQ).
/// </summary>
public class TicketCreationFailedEmailHandler : IEventHandler<SendTicketCreationFailedEmail>
{
    private readonly EmailService _emailService;
    private readonly ILogger<TicketCreationFailedEmailHandler> _logger;

    public TicketCreationFailedEmailHandler(EmailService emailService, ILogger<TicketCreationFailedEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(SendTicketCreationFailedEmail message, EventContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.CreatedByEmail))
        {
            _logger.LogWarning("Skipping ticket-creation-failed email for ticket {TicketId}: creator has no email address", message.TicketId);
            return;
        }

        var subject = $"We couldn't process your ticket: {message.Title}";
        var body = $"Hello {message.CreatedByName},<br/><br/>" +
                   $"Unfortunately we couldn't process your ticket \"{message.Title}\": {message.Reason}. " +
                   "Please try again or contact support.";

        await _emailService.SendEmailAsync(message.CreatedByEmail, subject, body);
    }
}
