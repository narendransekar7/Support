using System.Security.Cryptography;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus provider, mirroring Kafka:
/// - one topic per message type (the same names as the Kafka topics),
/// - one subscription per handler (= Kafka consumer group), each run by a ServiceBusProcessor that calls
///   the same <c>IEventHandler&lt;T&gt;</c>s,
/// - MaxDeliveryCount + the built-in dead-letter sub-queue instead of client-side retries + a ".dlt" topic.
/// </summary>
internal static class AzureServiceBusRegistration
{
    // Service Bus limits subscription names to 50 characters.
    private const int MaxSubscriptionNameLength = 50;

    public static void AddAzureServiceBusMessaging(this IServiceCollection services, IConfigurationSection section, MessagingBuilder builder)
    {
        var options = section.Get<AzureServiceBusOptions>() ?? new AzureServiceBusOptions();
        if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
            throw new InvalidOperationException(
                "Messaging:Provider is AzureServiceBus but neither Messaging:AzureServiceBus:ConnectionString " +
                "nor Messaging:AzureServiceBus:FullyQualifiedNamespace is set.");

        services.AddSingleton(Options.Create(options));
        services.AddSingleton(new MessagingServiceName(builder.ServiceName));
        services.AddSingleton<AzureServiceBusClientFactory>();
        services.AddSingleton<AzureServiceBusEntityManager>();
        // One client (one AMQP connection) per process, shared by every sender and processor.
        services.AddSingleton(sp => sp.GetRequiredService<AzureServiceBusClientFactory>().CreateClient());
        services.AddSingleton<IEventPublisher, AzureServiceBusEventPublisher>();

        var prefix = string.IsNullOrWhiteSpace(options.SubscriptionPrefix) ? builder.ServiceName : options.SubscriptionPrefix;
        foreach (var subscription in builder.Subscriptions)
        {
            // "<prefix>.<handler>", e.g. "ss-email-api.ticket-created-email-handler". The topic already
            // tells messages apart, so a handler of several messages (the saga orchestrator) or a generic
            // one (AuditEventHandler<T>) uses the same subscription name on each of its topics.
            var name = subscription.ConsumerGroup
                       ?? $"{prefix}.{MessageNaming.ToKebabCaseWithoutGenericArguments(subscription.HandlerType)}";
            subscription.AddServiceBusProcessor(services, FitSubscriptionName(name));
        }

        services.AddHealthChecks().AddCheck<AzureServiceBusHealthCheck>("azure-service-bus", tags: new[] { "ready", "messaging" });
    }

    private static string FitSubscriptionName(string name)
    {
        if (name.Length <= MaxSubscriptionNameLength)
            return name;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..8].ToLowerInvariant();
        return $"{name[..(MaxSubscriptionNameLength - 9)]}-{hash}";
    }
}

/// <summary>
/// Readiness: with management rights (AutoCreateEntities) reads the namespace properties, which needs a
/// working connection and credentials. Without them (emulator, send/listen-only policy) there's no
/// cheap call to make, so it only reports that the check was skipped.
/// </summary>
internal sealed class AzureServiceBusHealthCheck : IHealthCheck
{
    private readonly AzureServiceBusEntityManager _entities;

    public AzureServiceBusHealthCheck(AzureServiceBusEntityManager entities) => _entities = entities;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_entities.CanManage)
            return HealthCheckResult.Healthy("Not checked: AutoCreateEntities is off (no management access)");

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            var properties = await _entities.Admin.GetNamespacePropertiesAsync(timeout.Token);
            return HealthCheckResult.Healthy($"Service Bus namespace {properties.Value.Name} reachable");
        }
        catch (Exception ex) when (ex is ServiceBusException or OperationCanceledException or Azure.RequestFailedException or UnauthorizedAccessException)
        {
            return HealthCheckResult.Unhealthy("Service Bus unreachable", ex);
        }
    }
}
