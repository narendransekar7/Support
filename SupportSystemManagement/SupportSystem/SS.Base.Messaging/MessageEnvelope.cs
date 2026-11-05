using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging;

/// <summary>
/// What the topic-based providers (Kafka, Azure Service Bus) do the same way when they wrap a message
/// on publish and unwrap it on delivery. Only how the values travel differs per broker.
/// </summary>
internal static class MessageEnvelope
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// The key of a message published without one: ticket messages use their ticket id (so every step
    /// of one ticket's saga stays ordered within its topic), user messages their user id.
    /// </summary>
    public static string? DefaultKey(object message) =>
        (message as ITicketMessage)?.TicketId.ToString() ?? (message as IUserMessage)?.UserId.ToString();

    /// <summary>
    /// The published message id. A message without a usable one still gets a stable id derived from
    /// <paramref name="fallbackSource"/>: the same delivery always produces the same Guid, so
    /// de-duplication keeps working.
    /// </summary>
    public static Guid ParseEventId(string? value, string fallbackSource) =>
        Guid.TryParse(value, out var id) ? id : new Guid(MD5.HashData(Encoding.UTF8.GetBytes(fallbackSource)));

    public static DateTime ParseOccurredAt(string? value, DateTime fallback) =>
        DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out var occurredAt) ? occurredAt : fallback;
}
