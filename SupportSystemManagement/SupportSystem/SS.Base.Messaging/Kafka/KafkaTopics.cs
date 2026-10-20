namespace SS.Base.Messaging.Kafka;

/// <summary>
/// Kafka-specific names on top of <see cref="MessageNaming"/>: a topic's dead letters go to
/// "&lt;topic&gt;.dlt", e.g. "support.ticket-created.dlt".
/// </summary>
internal static class KafkaTopics
{
    public const string DeadLetterSuffix = ".dlt";

    public static string DeadLetterFor(string topic) => topic + DeadLetterSuffix;
}

/// <summary>
/// Header names on every Kafka message. Kafka itself only knows key, value, headers, timestamp; the
/// rest (id, type, trace context) is our convention.
/// </summary>
internal static class KafkaHeaders
{
    public const string MessageId = MessageHeaders.MessageId;
    public const string MessageType = MessageHeaders.MessageType;
    public const string OccurredAt = MessageHeaders.OccurredAt;
    public const string Source = MessageHeaders.Source;
    public const string TraceParent = MessageHeaders.TraceParent;

    // Added when a message is moved to a dead-letter topic.
    public const string DeadLetterReason = "dlt-reason";
    public const string DeadLetterConsumerGroup = "dlt-consumer-group";
    public const string DeadLetterOriginalTopic = "dlt-original-topic";
    public const string DeadLetterOriginalPartition = "dlt-original-partition";
    public const string DeadLetterOriginalOffset = "dlt-original-offset";
}
