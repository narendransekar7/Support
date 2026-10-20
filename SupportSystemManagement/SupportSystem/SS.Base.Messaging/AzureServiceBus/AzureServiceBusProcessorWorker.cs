using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// One Service Bus subscription: a ServiceBusProcessor on <typeparamref name="TMessage"/>'s topic,
/// subscription <c>subscription</c>, passing each message to <typeparamref name="THandler"/>.
///
/// Peek-lock, settled by hand (at-least-once, like the Kafka worker's manual commit):
/// - handler succeeded -> Complete (the message is removed from this subscription);
/// - handler failed -> wait n * RetryBackoffMs, then Abandon (the lock is released and the broker
///   redelivers it with DeliveryCount + 1);
/// - last allowed delivery failed, or the body can't be read -> DeadLetter with the reason. The
///   subscription's MaxDeliveryCount makes the broker do the same if this process dies mid-way.
/// Dead letters land in "&lt;topic&gt;/Subscriptions/&lt;subscription&gt;/$DeadLetterQueue" - Service Bus has it
/// built in, where Kafka needed a ".dlt" topic.
/// </summary>
internal sealed class AzureServiceBusProcessorWorker<TMessage, THandler> : BackgroundService
    where TMessage : class
    where THandler : class, IEventHandler<TMessage>
{
    private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(5);

    private readonly ServiceBusClient _client;
    private readonly AzureServiceBusEntityManager _entities;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AzureServiceBusOptions _options;
    private readonly ILogger _logger;
    private readonly string _topic;
    private readonly string _subscription;
    private readonly string _handlerName;

    public AzureServiceBusProcessorWorker(ServiceBusClient client, AzureServiceBusEntityManager entities, IServiceScopeFactory scopeFactory,
        IOptions<AzureServiceBusOptions> options, ILoggerFactory loggerFactory, string subscription)
    {
        _client = client;
        _entities = entities;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _subscription = subscription;
        _topic = MessageNaming.Topic(typeof(TMessage), _options.TopicPrefix);
        _handlerName = MessageNaming.ToKebabCase(typeof(THandler));
        _logger = loggerFactory.CreateLogger($"SS.Base.Messaging.AzureServiceBus.Processor.{_handlerName}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // (Re)start loop: the namespace may be unreachable at startup; never crash the host.
        while (!stoppingToken.IsCancellationRequested)
        {
            ServiceBusProcessor? processor = null;
            try
            {
                await _entities.EnsureSubscriptionAsync(_topic, _subscription, stoppingToken);

                processor = _client.CreateProcessor(_topic, _subscription, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,           // settle by hand, after the handler
                    MaxConcurrentCalls = _options.MaxConcurrentCalls,
                    PrefetchCount = 0,
                    // Keep the lock while a slow handler (or the retry back-off) runs.
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
                });
                processor.ProcessMessageAsync += args => ProcessAsync(args, stoppingToken);
                processor.ProcessErrorAsync += OnErrorAsync;

                await processor.StartProcessingAsync(stoppingToken);
                _logger.LogInformation("Subscription {Topic}/{Subscription} processing -> {Handler}", _topic, _subscription, _handlerName);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Bus processor for {Topic}/{Subscription} failed to start; retrying in {Delay}s",
                    _topic, _subscription, RestartDelay.TotalSeconds);
                try { await Task.Delay(RestartDelay, stoppingToken); } catch (OperationCanceledException) { }
            }
            finally
            {
                if (processor != null)
                {
                    try { await processor.StopProcessingAsync(CancellationToken.None); } catch (Exception ex) { _logger.LogDebug(ex, "Stopping processor failed"); }
                    await processor.DisposeAsync();
                }
            }
        }
    }

    private async Task ProcessAsync(ProcessMessageEventArgs args, CancellationToken stoppingToken)
    {
        var received = args.Message;
        var context = new EventContext(
            EventId: Guid.TryParse(received.MessageId, out var id) ? id : FallbackEventId(received.MessageId),
            OccurredAtUtc: received.ApplicationProperties.TryGetValue(MessageHeaders.OccurredAt, out var at)
                           && DateTime.TryParse(at?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var occurred)
                ? occurred : received.EnqueuedTime.UtcDateTime,
            Provider: nameof(MessagingProvider.AzureServiceBus),
            CorrelationId: received.CorrelationId);

        var traceParent = received.ApplicationProperties.TryGetValue(MessageHeaders.TraceParent, out var tp) ? tp?.ToString() : null;
        ActivityContext.TryParse(traceParent, null, out var parent);
        using var activity = MessagingTelemetry.Source.StartActivity($"{_topic} process", ActivityKind.Consumer, parent)
            // Not sampled: still continue the publisher's trace id for log correlation (see KafkaConsumerWorker).
            ?? (parent != default
                ? new Activity($"{_topic} process").SetParentId(parent.TraceId, parent.SpanId, parent.TraceFlags).Start()
                : null);
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.destination.name", _topic);
        activity?.SetTag("messaging.servicebus.subscription", _subscription);
        activity?.SetTag("messaging.servicebus.delivery_count", received.DeliveryCount);

        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object?>>
        {
            new("MessageId", context.EventId),
            new("ServiceBusTopic", _topic),
            new("ServiceBusSubscription", _subscription),
            new("DeliveryCount", received.DeliveryCount),
            new("SequenceNumber", received.SequenceNumber),
        });

        TMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<TMessage>(received.Body.ToMemory().Span, MessageNaming.Json)
                      ?? throw new JsonException("Message body was null.");
        }
        catch (JsonException ex)
        {
            // Retrying can't fix an unreadable message - straight to the dead-letter sub-queue.
            await args.DeadLetterMessageAsync(received, "DeserializationFailed", Truncate(ex.Message), stoppingToken);
            _logger.LogError(ex, "Dead-lettered unreadable {MessageType} {MessageId} on {Topic}/{Subscription}",
                typeof(TMessage).Name, context.EventId, _topic, _subscription);
            return;
        }

        try
        {
            await using var serviceScope = _scopeFactory.CreateAsyncScope();
            var handler = serviceScope.ServiceProvider.GetRequiredService<THandler>();
            await handler.HandleAsync(message, context, stoppingToken);

            await args.CompleteMessageAsync(received, stoppingToken);
            _logger.LogInformation("{Handler} handled {MessageType} {MessageId} from {Topic}/{Subscription} (delivery {DeliveryCount}, sequence {SequenceNumber})",
                _handlerName, typeof(TMessage).Name, context.EventId, _topic, _subscription, received.DeliveryCount, received.SequenceNumber);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            var maxDeliveries = _options.MaxRetries + 1;
            if (received.DeliveryCount >= maxDeliveries)
            {
                _logger.LogError(ex, "{Handler} failed {Deliveries} times on {MessageType} {MessageId}; dead-lettering it",
                    _handlerName, received.DeliveryCount, typeof(TMessage).Name, context.EventId);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await args.DeadLetterMessageAsync(received, "HandlerFailed", Truncate(ex.ToString()), CancellationToken.None);
                return;
            }

            var delay = TimeSpan.FromMilliseconds(_options.RetryBackoffMs * received.DeliveryCount);
            _logger.LogWarning(ex, "{Handler} failed on {MessageType} {MessageId} (delivery {Delivery} of {MaxDeliveries}); releasing it for redelivery in {Delay}ms",
                _handlerName, typeof(TMessage).Name, context.EventId, received.DeliveryCount, maxDeliveries, delay.TotalMilliseconds);
            await Task.Delay(delay, stoppingToken);
            await args.AbandonMessageAsync(received, cancellationToken: CancellationToken.None);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        // Connection drops and lock losses surface here; the processor recovers by itself.
        _logger.LogWarning("Service Bus processor error on {EntityPath} ({ErrorSource}): {Error}",
            args.EntityPath, args.ErrorSource, args.Exception.Message);
        return Task.CompletedTask;
    }

    // Dead-letter descriptions are limited to 4096 characters.
    private static string Truncate(string value) => value.Length > 4000 ? value[..4000] : value;

    private static Guid FallbackEventId(string messageId) => new(MD5.HashData(Encoding.UTF8.GetBytes(messageId)));
}
