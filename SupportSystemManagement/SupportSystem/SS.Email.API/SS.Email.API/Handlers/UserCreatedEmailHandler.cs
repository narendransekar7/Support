using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Messages.User;

namespace SS.Email.API.Handlers;

/// <summary>
/// "Welcome" email for a newly created user. Called by a Kafka consumer worker, a Service Bus processor
/// or UserCreatedEmailConsumer (RabbitMQ).
/// </summary>
public class UserCreatedEmailHandler : IEventHandler<UserCreated>
{
    private readonly EmailService _emailService;
    private readonly ILogger<UserCreatedEmailHandler> _logger;

    public UserCreatedEmailHandler(EmailService emailService, ILogger<UserCreatedEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreated message, EventContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Email))
        {
            _logger.LogWarning("Skipping welcome email for user {UserId}: no email address", message.UserId);
            return;
        }

        var subject = "Welcome to Support System!";
        var body = $"Hello {message.DisplayName},<br/><br/>Your account has been successfully created!";

        await _emailService.SendEmailAsync(message.Email, subject, body);
    }
}
