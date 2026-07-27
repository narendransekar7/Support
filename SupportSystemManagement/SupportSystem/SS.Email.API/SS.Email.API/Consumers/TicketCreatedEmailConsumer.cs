using MassTransit;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Consumers;

public class TicketCreatedEmailConsumer : IConsumer<TicketCreated>
{
    private readonly EmailService _emailService;

    public TicketCreatedEmailConsumer(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var message = context.Message;
        if (string.IsNullOrWhiteSpace(message.CreatedByEmail))
        {
            return;
        }

        var subject = $"Ticket received: {message.Title}";
        var body = $"Hello {message.CreatedByName},<br/><br/>" +
                   $"We've received your ticket \"{message.Title}\" (priority: {message.Priority}). " +
                   "We'll assign it to an engineer shortly.";

        await _emailService.SendEmailAsync(message.CreatedByEmail, subject, body);
    }
}
