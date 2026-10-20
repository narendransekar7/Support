using System.Text;
using System.Text.Json;

namespace SS.Base.Messaging;

/// <summary>
/// Names shared by the providers that address messages by topic (Kafka, Azure Service Bus): one topic
/// per message type, the same shape RabbitMQ gets from MassTransit (one exchange per type), so the
/// three are easy to compare. TicketCreated -> "support.ticket-created".
/// </summary>
internal static class MessageNaming
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string Topic(Type messageType, string prefix) => $"{prefix}.{ToKebabCase(messageType)}";

    /// <summary>Kebab-case type name; a generic type adds its arguments: AuditEventHandler&lt;TicketCreated&gt; -> "audit-event-handler.ticket-created".</summary>
    public static string ToKebabCase(Type type)
    {
        if (!type.IsGenericType)
            return ToKebabCase(type.Name);
        return string.Join(".", new[] { ToKebabCase(GenericName(type)) }.Concat(type.GetGenericArguments().Select(t => ToKebabCase(t))));
    }

    /// <summary>Kebab-case type name without generic arguments: AuditEventHandler&lt;TicketCreated&gt; -> "audit-event-handler".</summary>
    public static string ToKebabCaseWithoutGenericArguments(Type type) =>
        ToKebabCase(type.IsGenericType ? GenericName(type) : type.Name);

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

    private static string GenericName(Type type) => type.Name[..type.Name.IndexOf('`')];
}

/// <summary>The host's service name (Kafka client id / consumer-group prefix, Service Bus subscription prefix).</summary>
internal sealed record MessagingServiceName(string Value);

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
