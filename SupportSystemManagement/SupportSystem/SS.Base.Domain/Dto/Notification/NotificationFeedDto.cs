namespace SS.Base.Domain.Dto;

/// <summary>What the notification bell shows: the unread count and the newest notifications.</summary>
public class NotificationFeedDto
{
    public int UnreadCount { get; set; }
    public IEnumerable<NotificationDto> Items { get; set; } = Array.Empty<NotificationDto>();
}

public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public string Type { get; set; }
    public Guid? TicketId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
