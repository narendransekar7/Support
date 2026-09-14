using System.ComponentModel.DataAnnotations;

namespace SS.Base.Domain.Entities;

public class Notification
{
    [Key]
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public Guid TicketId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
