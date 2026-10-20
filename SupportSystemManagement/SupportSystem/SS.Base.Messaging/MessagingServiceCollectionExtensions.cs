using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Messaging.AzureServiceBus;
using SS.Base.Messaging.Kafka;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public const string SectionName = "Messaging";

    /// <summary>
    /// Wires up <see cref="IEventPublisher"/> and this host's consumers for the broker selected by
    /// `Messaging:Provider` (RabbitMq when unset, matching the system before Kafka was added).
    /// </summary>
    /// <param name="serviceName">Kafka client id and default consumer-group prefix, e.g. "ss-email-api".</param>
    public static MessagingProvider AddSupportSystemMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<MessagingBuilder>? configure = null)
    {
        var builder = new MessagingBuilder(services, serviceName);
        configure?.Invoke(builder);

        var provider = configuration.GetValue($"{SectionName}:Provider", MessagingProvider.RabbitMq);

        switch (provider)
        {
            case MessagingProvider.Kafka:
                services.AddKafkaMessaging(configuration.GetSection($"{SectionName}:Kafka"), builder);
                break;
            case MessagingProvider.RabbitMq:
                services.AddRabbitMqMessaging(configuration.GetSection("RabbitMq"), builder);
                break;
            case MessagingProvider.AzureServiceBus:
                services.AddAzureServiceBusMessaging(builder);
                break;
            default:
                throw new InvalidOperationException($"Unknown {SectionName}:Provider '{provider}'.");
        }

        services.AddSingleton<IHostedService>(sp => new MessagingStartupLog(
            provider, builder, sp.GetRequiredService<ILogger<MessagingStartupLog>>()));

        return provider;
    }

    /// <summary>
    /// For a host that never publishes or consumes (SS.Auth.Server.API) but references Application,
    /// whose MediatR scan registers handlers that depend on <see cref="IEventPublisher"/>. Keeps that
    /// host independent of any broker.
    /// </summary>
    public static IServiceCollection AddNoOpEventPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
        return services;
    }

    private sealed class NoOpEventPublisher : IEventPublisher
    {
        public Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class =>
            throw new InvalidOperationException($"This service has no message broker configured, so it can't publish {typeof(T).Name}.");
    }

    private sealed class MessagingStartupLog(MessagingProvider provider, MessagingBuilder builder, ILogger<MessagingStartupLog> logger) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Messaging provider: {Provider}; {Count} subscription(s): {Subscriptions}",
                provider, builder.Subscriptions.Count,
                string.Join(", ", builder.Subscriptions.Select(s => $"{s.MessageType.Name} -> {KafkaTopics.ToKebabCase(s.HandlerType)}")));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
