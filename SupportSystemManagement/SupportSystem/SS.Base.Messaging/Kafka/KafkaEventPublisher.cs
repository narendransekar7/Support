using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.Kafka;

/// <summary>
/// Kafka implementation of <see cref="IEventPublisher"/>: JSON value, message key for partitioning,
/// id/type/trace context in headers.
/// </summary>
internal sealed class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly KafkaTopicManager _topics;
    private readonly KafkaClientFactory _factory;
    private readonly KafkaServiceName _serviceName;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IProducer<string, byte[]> producer, KafkaTopicManager topics, KafkaClientFactory factory,
        KafkaServiceName serviceName, ILogger<KafkaEventPublisher> logger)
    {
        _producer = producer;
        _topics = topics;
        _factory = factory;
        _serviceName = serviceName;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string? key = null, CancellationToken cancellationToken = default) where T : class
    {
        var messageType = message.GetType();
        var topic = KafkaTopics.For(messageType, _factory.Options.TopicPrefix);
        await _topics.EnsureTopicAsync(topic);

        // Same key -> same partition -> consumed in order. Ticket messages default to their ticket id,
        // so every step of one ticket's saga stays ordered within its topic.
        key ??= (message as ITicketMessage)?.TicketId.ToString();

        using var activity = MessagingTelemetry.Source.StartActivity($"{topic} publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", topic);
        activity?.SetTag("messaging.kafka.message.key", key);

        var messageId = Guid.NewGuid();
        var headers = new Headers
        {
            { KafkaHeaders.MessageId, Encoding.UTF8.GetBytes(messageId.ToString()) },
            { KafkaHeaders.MessageType, Encoding.UTF8.GetBytes(messageType.FullName ?? messageType.Name) },
            { KafkaHeaders.OccurredAt, Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) },
            { KafkaHeaders.Source, Encoding.UTF8.GetBytes(_serviceName.Value) },
        };
        var traceParent = Activity.Current?.Id;
        if (traceParent != null)
            headers.Add(KafkaHeaders.TraceParent, Encoding.UTF8.GetBytes(traceParent));

        var result = await _producer.ProduceAsync(topic, new Message<string, byte[]>
        {
            Key = key!,
            Value = JsonSerializer.SerializeToUtf8Bytes(message, messageType, KafkaTopics.Json),
            Headers = headers,
        }, cancellationToken);

        activity?.SetTag("messaging.destination.partition.id", result.Partition.Value);
        _logger.LogInformation("Published {MessageType} {MessageId} to {Topic} partition {Partition} offset {Offset} (key {Key})",
            messageType.Name, messageId, topic, result.Partition.Value, result.Offset.Value, key);
    }
}
