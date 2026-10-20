using System.ComponentModel.DataAnnotations;

namespace SS.Base.Domain.Entities;

public class Notification
{
    [Key]
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }

    // What the notification is about - see NotificationType. The UI picks its icon and link from it.
    public string Type { get; set; } = NotificationType.TicketCreated;

    // The ticket it links to; null for notifications that aren't about a ticket (e.g. Welcome).
    public Guid? TicketId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    // Id of the message that created this notification (unique when set) - a redelivered
    // message finds it and doesn't create a second notification.
    public Guid? SourceEventId { get; set; }
}

/// <summary>Values of <see cref="Notification.Type"/>.</summary>
public static class NotificationType
{
    public const string TicketCreated = "TicketCreated";
    public const string Welcome = "Welcome";
}
