using MassTransit;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Email.API.Consumers;

public class TicketCreationFailedEmailConsumer : IConsumer<SendTicketCreationFailedEmail>
{
    private readonly EmailService _emailService;

    public TicketCreationFailedEmailConsumer(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<SendTicketCreationFailedEmail> context)
    {
        var message = context.Message;
        if (string.IsNullOrWhiteSpace(message.CreatedByEmail))
        {
            return;
        }

        var subject = $"We couldn't process your ticket: {message.Title}";
        var body = $"Hello {message.CreatedByName},<br/><br/>" +
                   $"Unfortunately we couldn't process your ticket \"{message.Title}\": {message.Reason}. " +
                   "Please try again or contact support.";

        await _emailService.SendEmailAsync(message.CreatedByEmail, subject, body);
    }
}
