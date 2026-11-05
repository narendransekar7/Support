using System.Text;

namespace SS.Base.Messaging;

/// <summary>
/// Names shared by the providers that address messages by topic (Kafka, Azure Service Bus): one topic
/// per message type, the same shape RabbitMQ gets from MassTransit (one exchange per type), so the
/// three are easy to compare. TicketCreated -> "support.ticket-created".
/// </summary>
internal static class MessageNaming
{
    public static string Topic(Type messageType, string prefix) => $"{prefix}.{ToKebabCase(messageType)}";

    /// <summary>Kebab-case type name; a generic type adds its arguments: AuditEventHandler&lt;TicketCreated&gt; -> "audit-event-handler.ticket-created".</summary>
    public static string ToKebabCase(Type type) =>
        string.Join(".", type.GetGenericArguments().Select(ToKebabCase).Prepend(ToKebabCaseWithoutGenericArguments(type)));

    /// <summary>Kebab-case type name without generic arguments: AuditEventHandler&lt;TicketCreated&gt; -> "audit-event-handler".</summary>
    public static string ToKebabCaseWithoutGenericArguments(Type type) =>
        ToKebabCase(type.IsGenericType ? type.Name[..type.Name.IndexOf('`')] : type.Name);

    private static string ToKebabCase(string name)
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
}
