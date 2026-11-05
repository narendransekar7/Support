namespace SS.Base.Messaging;

/// <summary>The host's service name (Kafka client id / consumer-group prefix, Service Bus subscription prefix).</summary>
internal sealed record MessagingServiceName(string Value);
