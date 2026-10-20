using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.Kafka;

internal static class KafkaRegistration
{
    public static void AddKafkaMessaging(this IServiceCollection services, IConfigurationSection section, MessagingBuilder builder)
    {
        services.Configure<KafkaOptions>(section);
        services.AddSingleton(new MessagingServiceName(builder.ServiceName));
        services.AddSingleton<KafkaClientFactory>();
        services.AddSingleton<KafkaTopicManager>();

        // One producer per process: it's thread-safe and batches across callers.
        services.AddSingleton(sp => sp.GetRequiredService<KafkaClientFactory>()
            .CreateProducer(sp.GetRequiredService<ILoggerFactory>().CreateLogger("SS.Base.Messaging.Kafka.Producer")));
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        var groupPrefix = section["ConsumerGroup"];
        if (string.IsNullOrWhiteSpace(groupPrefix))
            groupPrefix = builder.ServiceName;

        // Default group per handler: "<prefix>.<handler>", e.g. "ss-email-api.ticket-created-email-handler".
        // A handler subscribed to several messages (the saga orchestrator) gets one group per message,
        // "<prefix>.<handler>.<message>", so each topic is consumed and rebalanced independently.
        var handlersWithSeveralMessages = builder.Subscriptions.GroupBy(s => s.HandlerType)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

        foreach (var subscription in builder.Subscriptions)
        {
            var groupId = subscription.ConsumerGroup ?? $"{groupPrefix}.{MessageNaming.ToKebabCase(subscription.HandlerType)}";
            if (subscription.ConsumerGroup == null && handlersWithSeveralMessages.Contains(subscription.HandlerType))
                groupId += "." + MessageNaming.ToKebabCase(subscription.MessageType);
            subscription.AddKafkaWorker(services, groupId);
        }

        // A broker outage reports Degraded, not Unhealthy: /health/ready still answers 200, so the APIs keep serving
        // HTTP (reads, sign-in, ...) while consumers reconnect; only publishing fails until the brokers are back.
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", failureStatus: HealthStatus.Degraded, tags: new[] { "ready", "messaging" });
    }
}

internal sealed class KafkaHealthCheck : IHealthCheck
{
    private readonly KafkaTopicManager _topics;

    public KafkaHealthCheck(KafkaTopicManager topics) => _topics = topics;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await Task.Run(() => _topics.Admin.GetMetadata(TimeSpan.FromSeconds(3)), cancellationToken);
            return metadata.Brokers.Count > 0
                ? HealthCheckResult.Healthy($"{metadata.Brokers.Count} Kafka broker(s) reachable")
                : new HealthCheckResult(context.Registration.FailureStatus, "No Kafka brokers in metadata");
        }
        catch (KafkaException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Kafka unreachable", ex);
        }
    }
}
