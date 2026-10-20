using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.Kafka;

/// <summary>
/// One Kafka consumer: reads <typeparamref name="TMessage"/>'s topic as consumer group
/// <c>groupId</c> and passes each message to <typeparamref name="THandler"/>.
///
/// Delivery is at-least-once: the offset is committed only after the handler succeeded (or the
/// message was moved to the dead-letter topic). A crash in between means the message is read again,
/// which is why handlers de-duplicate on <see cref="EventContext.EventId"/>.
///
/// A handler that keeps failing is retried <see cref="KafkaOptions.MaxRetries"/> times with a growing
/// delay, then the message is copied to "&lt;topic&gt;.dlt" with the error in its headers and the
/// consumer moves on - one bad message doesn't block its partition forever.
/// </summary>
internal sealed class KafkaConsumerWorker<TMessage, THandler> : BackgroundService
    where TMessage : class
    where THandler : class, IEventHandler<TMessage>
{
    private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaClientFactory _factory;
    private readonly KafkaTopicManager _topics;
    private readonly IProducer<string, byte[]> _producer;
    private readonly ILogger _logger;
    private readonly string _groupId;
    private readonly string _topic;
    private readonly string _deadLetterTopic;
    private readonly string _handlerName;

    public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, KafkaClientFactory factory, KafkaTopicManager topics,
        IProducer<string, byte[]> producer, ILoggerFactory loggerFactory, string groupId)
    {
        _scopeFactory = scopeFactory;
        _factory = factory;
        _topics = topics;
        _producer = producer;
        _groupId = groupId;
        _handlerName = KafkaTopics.ToKebabCase(typeof(THandler));
        _logger = loggerFactory.CreateLogger($"SS.Base.Messaging.Kafka.Consumer.{_handlerName}");
        _topic = KafkaTopics.For(typeof(TMessage), factory.Options.TopicPrefix);
        _deadLetterTopic = KafkaTopics.DeadLetterFor(_topic);
    }

    // Consume() blocks, so run the loop on its own thread instead of holding up host startup.
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Factory.StartNew(() => RunAsync(stoppingToken), stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        // Outer loop: (re)connect. The broker may not be up yet, or may go away; never crash the host.
        while (!stoppingToken.IsCancellationRequested)
        {
            IConsumer<string, byte[]>? consumer = null;
            try
            {
                await _topics.EnsureTopicAsync(_topic);
                await _topics.EnsureTopicAsync(_deadLetterTopic);

                consumer = _factory.CreateConsumer(_groupId, _logger);
                consumer.Subscribe(_topic);
                _logger.LogInformation("Consumer group {GroupId} subscribed to {Topic} -> {Handler}", _groupId, _topic, _handlerName);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message == null)
                        continue;

                    await ProcessAsync(result, stoppingToken);

                    // Commit "next offset to read" for this partition. Only now is the message done for this group.
                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer {GroupId} on {Topic} failed; reconnecting in {Delay}s", _groupId, _topic, RestartDelay.TotalSeconds);
                await DelayIgnoringCancellation(RestartDelay, stoppingToken);
            }
            finally
            {
                if (consumer != null)
                {
                    // Close() leaves the group cleanly so its partitions are reassigned right away.
                    try { consumer.Close(); } catch (Exception ex) { _logger.LogDebug(ex, "Closing Kafka consumer {GroupId} failed", _groupId); }
                    consumer.Dispose();
                }
            }
        }
    }

    private async Task ProcessAsync(ConsumeResult<string, byte[]> result, CancellationToken stoppingToken)
    {
        var headers = result.Message.Headers;
        var context = new EventContext(
            EventId: Guid.TryParse(GetHeader(headers, KafkaHeaders.MessageId), out var id) ? id : FallbackEventId(result),
            OccurredAtUtc: DateTime.TryParse(GetHeader(headers, KafkaHeaders.OccurredAt), null, System.Globalization.DateTimeStyles.RoundtripKind, out var at)
                ? at : result.Message.Timestamp.UtcDateTime,
            Provider: nameof(MessagingProvider.Kafka),
            CorrelationId: result.Message.Key);

        ActivityContext.TryParse(GetHeader(headers, KafkaHeaders.TraceParent), null, out var parent);
        using var activity = MessagingTelemetry.Source.StartActivity($"{_topic} process", ActivityKind.Consumer, parent)
            // Not sampled (e.g. no exporter configured, so the HTTP request upstream wasn't recorded): still
            // continue the publisher's trace id, so log lines across services share one TraceId and any
            // message this handler publishes carries it on.
            ?? (parent != default
                ? new Activity($"{_topic} process").SetParentId(parent.TraceId, parent.SpanId, parent.TraceFlags).Start()
                : null);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", _topic);
        activity?.SetTag("messaging.consumer.group.name", _groupId);
        activity?.SetTag("messaging.destination.partition.id", result.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", result.Offset.Value);

        // A list of pairs (not a Dictionary), which the JSON console formatter writes out as fields.
        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object?>>
        {
            new("MessageId", context.EventId),
            new("KafkaTopic", _topic),
            new("KafkaPartition", result.Partition.Value),
            new("KafkaOffset", result.Offset.Value),
        });

        TMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<TMessage>(result.Message.Value, KafkaTopics.Json);
            if (message == null)
                throw new JsonException("Message value was null.");
        }
        catch (JsonException ex)
        {
            // Retrying can't fix an unreadable message - straight to the dead-letter topic.
            await DeadLetterAsync(result, $"Deserialization failed: {ex.Message}");
            return;
        }

        var maxRetries = _factory.Options.MaxRetries;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var serviceScope = _scopeFactory.CreateAsyncScope();
                var handler = serviceScope.ServiceProvider.GetRequiredService<THandler>();
                await handler.HandleAsync(message, context, stoppingToken);

                _logger.LogInformation("{Handler} handled {MessageType} {MessageId} from {Topic} partition {Partition} offset {Offset}",
                    _handlerName, typeof(TMessage).Name, context.EventId, _topic, result.Partition.Value, result.Offset.Value);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                if (attempt > maxRetries)
                {
                    _logger.LogError(ex, "{Handler} failed {Attempts} times on {MessageType} {MessageId}; moving it to {DeadLetterTopic}",
                        _handlerName, attempt, typeof(TMessage).Name, context.EventId, _deadLetterTopic);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await DeadLetterAsync(result, ex.ToString());
                    return;
                }

                var delay = TimeSpan.FromMilliseconds(_factory.Options.RetryBackoffMs * attempt);
                _logger.LogWarning(ex, "{Handler} failed on {MessageType} {MessageId} (attempt {Attempt} of {MaxAttempts}); retrying in {Delay}ms",
                    _handlerName, typeof(TMessage).Name, context.EventId, attempt, maxRetries + 1, delay.TotalMilliseconds);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private async Task DeadLetterAsync(ConsumeResult<string, byte[]> result, string reason)
    {
        var headers = new Headers();
        foreach (var header in result.Message.Headers)
            headers.Add(header.Key, header.GetValueBytes());

        headers.Add(KafkaHeaders.DeadLetterReason, Encoding.UTF8.GetBytes(reason.Length > 4000 ? reason[..4000] : reason));
        headers.Add(KafkaHeaders.DeadLetterConsumerGroup, Encoding.UTF8.GetBytes(_groupId));
        headers.Add(KafkaHeaders.DeadLetterOriginalTopic, Encoding.UTF8.GetBytes(_topic));
        headers.Add(KafkaHeaders.DeadLetterOriginalPartition, Encoding.UTF8.GetBytes(result.Partition.Value.ToString()));
        headers.Add(KafkaHeaders.DeadLetterOriginalOffset, Encoding.UTF8.GetBytes(result.Offset.Value.ToString()));

        // If this throws, the offset isn't committed and the message is read again after reconnecting.
        await _producer.ProduceAsync(_deadLetterTopic, new Message<string, byte[]>
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = headers,
        });
    }

    private static string? GetHeader(Headers? headers, string name) =>
        headers != null && headers.TryGetLastBytes(name, out var bytes) ? Encoding.UTF8.GetString(bytes) : null;

    // Messages published without our headers still get a stable id: the same topic/partition/offset
    // always produces the same Guid, so de-duplication keeps working.
    private static Guid FallbackEventId(ConsumeResult<string, byte[]> result) =>
        new(MD5.HashData(Encoding.UTF8.GetBytes($"{result.Topic}/{result.Partition.Value}/{result.Offset.Value}")));

    private static async Task DelayIgnoringCancellation(TimeSpan delay, CancellationToken token)
    {
        try { await Task.Delay(delay, token); } catch (OperationCanceledException) { }
    }
}
