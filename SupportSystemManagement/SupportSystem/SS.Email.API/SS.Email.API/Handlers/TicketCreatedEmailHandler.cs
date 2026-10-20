using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Handlers;

/// <summary>
/// "Ticket received" email - the independent email branch reacting to TicketCreated.
/// Called by a Kafka consumer worker or by TicketCreatedEmailConsumer (RabbitMQ).
/// </summary>
public class TicketCreatedEmailHandler : IEventHandler<TicketCreated>
{
    private readonly EmailService _emailService;
    private readonly ILogger<TicketCreatedEmailHandler> _logger;

    public TicketCreatedEmailHandler(EmailService emailService, ILogger<TicketCreatedEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(TicketCreated message, EventContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.CreatedByEmail))
        {
            _logger.LogWarning("Skipping ticket-received email for ticket {TicketId}: creator has no email address", message.TicketId);
            return;
        }

        var subject = $"Ticket received: {message.Title}";
        var body = $"Hello {message.CreatedByName},<br/><br/>" +
                   $"We've received your ticket \"{message.Title}\" (priority: {message.Priority}). " +
                   "We'll assign it to an engineer shortly.";

        await _emailService.SendEmailAsync(message.CreatedByEmail, subject, body);
    }
}
