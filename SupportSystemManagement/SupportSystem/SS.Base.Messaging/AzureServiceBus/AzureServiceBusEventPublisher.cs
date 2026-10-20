using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus implementation of <see cref="IEventPublisher"/>: JSON body sent to the message
/// type's topic. MessageId, Subject (type name) and CorrelationId (the key) are native Service Bus
/// properties; occurred-at/source/traceparent go in application properties.
/// </summary>
internal sealed class AzureServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly AzureServiceBusEntityManager _entities;
    private readonly AzureServiceBusOptions _options;
    private readonly MessagingServiceName _serviceName;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;

    // A sender holds an AMQP link to one topic; they're thread-safe and meant to be reused.
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public AzureServiceBusEventPublisher(ServiceBusClient client, AzureServiceBusEntityManager entities, IOptions<AzureServiceBusOptions> options,
        MessagingServiceName serviceName, ILogger<AzureServiceBusEventPublisher> logger)
    {
        _client = client;
        _entities = entities;
        _options = options.Value;
        _serviceName = serviceName;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        var messageType = message.GetType();
        var topic = MessageNaming.Topic(messageType, _options.TopicPrefix);
        await _entities.EnsureTopicAsync(topic, cancellationToken);

        key ??= (message as ITicketMessage)?.TicketId.ToString() ?? (message as IUserMessage)?.UserId.ToString();

        using var activity = MessagingTelemetry.Source.StartActivity($"{topic} publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.destination.name", topic);

        var messageId = Guid.NewGuid();
        var serviceBusMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(message, messageType, MessageNaming.Json))
        {
            // Stable across redeliveries -> the handlers' de-duplication key (and the broker's, if duplicate detection is on).
            MessageId = messageId.ToString(),
            Subject = messageType.Name,
            ContentType = "application/json",
            CorrelationId = key,
        };
        serviceBusMessage.ApplicationProperties[MessageHeaders.MessageType] = messageType.FullName ?? messageType.Name;
        serviceBusMessage.ApplicationProperties[MessageHeaders.OccurredAt] = DateTime.UtcNow.ToString("O");
        serviceBusMessage.ApplicationProperties[MessageHeaders.Source] = _serviceName.Value;
        if (Activity.Current?.Id is { } traceParent)
            serviceBusMessage.ApplicationProperties[MessageHeaders.TraceParent] = traceParent;

        var sender = _senders.GetOrAdd(topic, _client.CreateSender);
        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

        _logger.LogInformation("Published {MessageType} {MessageId} to Service Bus topic {Topic} (correlation {Key})",
            messageType.Name, messageId, topic, key);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();
    }
}
