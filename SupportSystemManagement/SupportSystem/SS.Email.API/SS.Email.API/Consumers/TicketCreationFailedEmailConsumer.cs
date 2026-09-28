using MassTransit;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Consumers;

public class TicketCreationFailedEmailConsumer : IConsumer<SendTicketCreationFailedEmail>
{
    private readonly EmailService _emailService;
    private readonly ILogger<TicketCreationFailedEmailConsumer> _logger;

    public TicketCreationFailedEmailConsumer(EmailService emailService, ILogger<TicketCreationFailedEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendTicketCreationFailedEmail> context)
    {
        var message = context.Message;
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
