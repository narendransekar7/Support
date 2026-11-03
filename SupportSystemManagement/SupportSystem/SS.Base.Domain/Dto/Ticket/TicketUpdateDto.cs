namespace SS.Base.Domain.Dto;

public class TicketUpdateDto
{
    public Guid UpdateId { get; set; }
    public string Content { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpdatedBy { get; set; }
}