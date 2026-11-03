namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
public class GetTicketByIdQuery : IRequest<Ticket>
{
    public int TicketId { get; set; }

    public GetTicketByIdQuery(int ticketId)
    {
        TicketId = ticketId;
    }
}