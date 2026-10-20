using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Creates topics and subscriptions through the management API (when AutoCreateEntities is on), each
/// once per process. A subscription is Service Bus' equivalent of a Kafka consumer group: every
/// subscription of a topic gets its own copy of every message.
/// </summary>
internal sealed class AzureServiceBusEntityManager
{
    private readonly AzureServiceBusOptions _options;
    private readonly Lazy<ServiceBusAdministrationClient> _admin;
    private readonly ILogger<AzureServiceBusEntityManager> _logger;
    private readonly ConcurrentDictionary<string, byte> _ensured = new();

    public AzureServiceBusEntityManager(IOptions<AzureServiceBusOptions> options, AzureServiceBusClientFactory factory,
        ILogger<AzureServiceBusEntityManager> logger)
    {
        _options = options.Value;
        _admin = new Lazy<ServiceBusAdministrationClient>(factory.CreateAdministrationClient);
        _logger = logger;
    }

    public bool CanManage => _options.AutoCreateEntities;

    public ServiceBusAdministrationClient Admin => _admin.Value;

    public async Task EnsureTopicAsync(string topic, CancellationToken cancellationToken)
    {
        if (!_options.AutoCreateEntities || _ensured.ContainsKey(topic))
            return;

        if (!await Admin.TopicExistsAsync(topic, cancellationToken))
        {
            try
            {
                await Admin.CreateTopicAsync(new CreateTopicOptions(topic)
                {
                    // Like Kafka's 7-day retention - though unlike Kafka a message is gone once every subscription has completed it.
                    DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                }, cancellationToken);
                _logger.LogInformation("Created Service Bus topic {Topic}", topic);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Another service created it first - fine.
            }
        }

        _ensured.TryAdd(topic, 0);
    }

    public async Task EnsureSubscriptionAsync(string topic, string subscription, CancellationToken cancellationToken)
    {
        var key = $"{topic}/{subscription}";
        if (!_options.AutoCreateEntities || _ensured.ContainsKey(key))
            return;

        await EnsureTopicAsync(topic, cancellationToken);

        if (!await Admin.SubscriptionExistsAsync(topic, subscription, cancellationToken))
        {
            try
            {
                await Admin.CreateSubscriptionAsync(new CreateSubscriptionOptions(topic, subscription)
                {
                    // After this many deliveries the broker moves the message to the subscription's
                    // dead-letter sub-queue by itself (the processor also dead-letters explicitly on the last one).
                    MaxDeliveryCount = _options.MaxRetries + 1,
                    LockDuration = TimeSpan.FromMinutes(1),
                    DeadLetteringOnMessageExpiration = true,
                }, cancellationToken);
                _logger.LogInformation("Created Service Bus subscription {Topic}/{Subscription} (max delivery count {MaxDeliveryCount})",
                    topic, subscription, _options.MaxRetries + 1);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
            }
        }

        _ensured.TryAdd(key, 0);
    }
}
