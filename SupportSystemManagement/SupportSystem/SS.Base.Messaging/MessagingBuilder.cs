using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Messaging.AzureServiceBus;
using SS.Base.Messaging.Kafka;

namespace SS.Base.Messaging;

/// <summary>
/// What one host receives. A host lists:
/// - <see cref="Subscribe{TMessage,THandler}"/>: provider-neutral handlers. With Kafka each becomes a
///   consumer (its own consumer group) on the message's topic; with Azure Service Bus a processor on its
///   own subscription of the message's topic; with RabbitMQ the MassTransit consumers registered in
///   <see cref="ForRabbitMq"/> call the same handlers.
/// - <see cref="ForRabbitMq"/>: the MassTransit consumers/sagas, used only when RabbitMQ is selected.
/// </summary>
public sealed class MessagingBuilder
{
    internal MessagingBuilder(IServiceCollection services, string serviceName)
    {
        Services = services;
        ServiceName = serviceName;
    }

    internal IServiceCollection Services { get; }
    internal string ServiceName { get; }
    internal List<Subscription> Subscriptions { get; } = new();
    internal List<Action<IBusRegistrationConfigurator>> RabbitMqRegistrations { get; } = new();

    /// <summary>
    /// Handle <typeparamref name="TMessage"/> with <typeparamref name="THandler"/>.
    /// </summary>
    /// <param name="consumerGroup">
    /// Kafka consumer group / Service Bus subscription name. Defaults to "&lt;prefix&gt;.&lt;handler name&gt;"
    /// (prefix = the service name), so every handler gets its own copy of every message.
    /// </param>
    public MessagingBuilder Subscribe<TMessage, THandler>(string? consumerGroup = null)
        where TMessage : class
        where THandler : class, IEventHandler<TMessage>
    {
        // Registered for every provider: the RabbitMQ consumers resolve the same handlers from DI.
        Services.AddScoped<THandler>();
        Subscriptions.Add(new Subscription(typeof(TMessage), typeof(THandler), consumerGroup,
            AddKafkaWorker: (services, group) => services.AddSingleton<IHostedService>(sp =>
                ActivatorUtilities.CreateInstance<KafkaConsumerWorker<TMessage, THandler>>(sp, group)),
            AddServiceBusProcessor: (services, subscription) => services.AddSingleton<IHostedService>(sp =>
                ActivatorUtilities.CreateInstance<AzureServiceBusProcessorWorker<TMessage, THandler>>(sp, subscription))));
        return this;
    }

    /// <summary>
    /// MassTransit registrations (consumers, saga state machines) used only with the RabbitMQ provider.
    /// </summary>
    public MessagingBuilder ForRabbitMq(Action<IBusRegistrationConfigurator> configure)
    {
        RabbitMqRegistrations.Add(configure);
        return this;
    }
}

internal sealed record Subscription(
    Type MessageType,
    Type HandlerType,
    string? ConsumerGroup,
    Action<IServiceCollection, string> AddKafkaWorker,
    Action<IServiceCollection, string> AddServiceBusProcessor);
