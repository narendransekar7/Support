using MassTransit;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Consumers;

public class TicketCreatedEmailConsumer : IConsumer<TicketCreated>
{
    private readonly EmailService _emailService;
    private readonly ILogger<TicketCreatedEmailConsumer> _logger;

    public TicketCreatedEmailConsumer(EmailService emailService, ILogger<TicketCreatedEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var message = context.Message;
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
