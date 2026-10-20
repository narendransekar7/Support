using System.Text;
using System.Text.Json;

namespace SS.Base.Messaging.Kafka;

/// <summary>
/// One topic per message type, the same shape RabbitMQ (one exchange per type via MassTransit) and
/// Azure Service Bus (one topic per type + a subscription per service) use - so the three are easy
/// to compare. TicketCreated -> "support.ticket-created"; its dead letters -> "support.ticket-created.dlt".
/// </summary>
internal static class KafkaTopics
{
    public const string DeadLetterSuffix = ".dlt";

    public static string For(Type messageType, string prefix) => $"{prefix}.{ToKebabCase(messageType)}";

    public static string DeadLetterFor(string topic) => topic + DeadLetterSuffix;

    /// <summary>Kebab-case type name; a generic type adds its arguments: AuditEventHandler&lt;TicketCreated&gt; -> "audit-event-handler.ticket-created".</summary>
    public static string ToKebabCase(Type type)
    {
        if (!type.IsGenericType)
            return ToKebabCase(type.Name);
        var name = type.Name[..type.Name.IndexOf('`')];
        return string.Join(".", new[] { ToKebabCase(name) }.Concat(type.GetGenericArguments().Select(t => ToKebabCase(t))));
    }

    public static string ToKebabCase(string name)
    {
        var sb = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Header names on every Kafka message. Kafka itself only knows key, value, headers, timestamp; the
/// rest (id, type, trace context) is our convention.
/// </summary>
internal static class KafkaHeaders
{
    public const string MessageId = "message-id";
    public const string MessageType = "message-type";
    public const string OccurredAt = "occurred-at";
    public const string Source = "source";
    public const string TraceParent = "traceparent";

    // Added when a message is moved to a dead-letter topic.
    public const string DeadLetterReason = "dlt-reason";
    public const string DeadLetterConsumerGroup = "dlt-consumer-group";
    public const string DeadLetterOriginalTopic = "dlt-original-topic";
    public const string DeadLetterOriginalPartition = "dlt-original-partition";
    public const string DeadLetterOriginalOffset = "dlt-original-offset";
}
