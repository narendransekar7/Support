namespace SS.Base.Domain.Dto;

public class TicketDto
{
    public int TicketId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string Visibility { get; set; }
    public int CreatedBy { get; set; }
    public int? AssignedTo { get; set; }

    public List<TicketUpdateDto> Updates { get; set; }
}