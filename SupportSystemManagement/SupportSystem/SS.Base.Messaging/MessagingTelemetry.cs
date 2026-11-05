using System.Diagnostics;

namespace SS.Base.Messaging;

/// <summary>
/// Spans for Kafka and Service Bus publish/process, so a ticket's trace continues from the Ticket API
/// through every consumer service (the `traceparent` header carries the context across the broker).
/// MassTransit emits its own spans for RabbitMQ under the "MassTransit" source.
/// </summary>
public static class MessagingTelemetry
{
    public const string ActivitySourceName = "SS.Base.Messaging";

    internal static readonly ActivitySource Source = new(ActivitySourceName);

    /// <summary>Starts the span of one delivery, as a child of the publisher's <paramref name="traceParent"/>.</summary>
    internal static Activity? StartProcessActivity(string topic, string? traceParent)
    {
        ActivityContext.TryParse(traceParent, null, out var parent);
        return Source.StartActivity($"{topic} process", ActivityKind.Consumer, parent)
            // Not sampled (e.g. no exporter configured, so the HTTP request upstream wasn't recorded): still
            // continue the publisher's trace id, so log lines across services share one TraceId and any
            // message this handler publishes carries it on.
            ?? (parent != default
                ? new Activity($"{topic} process").SetParentId(parent.TraceId, parent.SpanId, parent.TraceFlags).Start()
                : null);
    }
}
