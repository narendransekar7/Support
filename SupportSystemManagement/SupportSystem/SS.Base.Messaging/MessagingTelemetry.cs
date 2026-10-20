using System.Diagnostics;

namespace SS.Base.Messaging;

/// <summary>
/// Spans for Kafka publish/process, so a ticket's trace continues from the Ticket API through every
/// consumer service (the `traceparent` header carries the context across the broker). MassTransit
/// emits its own spans for RabbitMQ under the "MassTransit" source.
/// </summary>
public static class MessagingTelemetry
{
    public const string ActivitySourceName = "SS.Base.Messaging";

    internal static readonly ActivitySource Source = new(ActivitySourceName);
}
