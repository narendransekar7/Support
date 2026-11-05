namespace SS.Base.Messaging;

/// <summary>
/// Message property / header names every topic-based provider writes. The broker only knows the body
/// (and, for Service Bus, MessageId/Subject/CorrelationId); the rest is our convention.
/// </summary>
internal static class MessageHeaders
{
    public const string MessageId = "message-id";
    public const string MessageType = "message-type";
    public const string OccurredAt = "occurred-at";
    public const string Source = "source";
    public const string TraceParent = "traceparent";
}
