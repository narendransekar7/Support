namespace SS.Base.Domain.Dto;

public class TicketDto
{
    public Guid TicketId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string Visibility { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid AssignedTo { get; set; }

    public List<TicketUpdateDto> Updates { get; set; }
}