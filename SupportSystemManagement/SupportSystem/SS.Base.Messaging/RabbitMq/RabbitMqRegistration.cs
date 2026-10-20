using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.RabbitMq;

/// <summary>
/// The original MassTransit-over-RabbitMQ setup (previously repeated in each host's Program.cs),
/// unchanged in behavior: the host's consumers/sagas from <see cref="MessagingBuilder.ForRabbitMq"/>,
/// convention-based endpoints, and the `RabbitMq` config section.
/// </summary>
internal static class RabbitMqRegistration
{
    public static void AddRabbitMqMessaging(this IServiceCollection services, IConfigurationSection rabbitMqConfig, MessagingBuilder builder)
    {
        services.AddMassTransit(x =>
        {
            foreach (var register in builder.RabbitMqRegistrations)
                register(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfig["Host"], rabbitMqConfig["VirtualHost"], h =>
                {
                    h.Username(rabbitMqConfig["Username"]);
                    h.Password(rabbitMqConfig["Password"]);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
    }
}

/// <summary>
/// RabbitMQ implementation of <see cref="IEventPublisher"/>: a MassTransit Publish (fan-out exchange
/// per message type). Inside a consumer, the scoped IPublishEndpoint is that consumer's context, so
/// follow-up messages keep the conversation/correlation ids. The key is a Kafka concept and is ignored.
/// </summary>
internal sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class =>
        _publishEndpoint.Publish(message, cancellationToken);
}

public static class ConsumeContextExtensions
{
    /// <summary>
    /// The broker-neutral <see cref="EventContext"/> for a MassTransit delivery, so a MassTransit
    /// consumer can hand its message to an <see cref="IEventHandler{T}"/>.
    /// </summary>
    public static EventContext ToEventContext(this ConsumeContext context) => new(
        context.MessageId ?? Guid.NewGuid(),
        context.SentTime ?? DateTime.UtcNow,
        nameof(MessagingProvider.RabbitMq),
        context.CorrelationId?.ToString());
}
