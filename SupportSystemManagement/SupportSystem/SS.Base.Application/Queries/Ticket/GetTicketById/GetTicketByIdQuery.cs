namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
public class GetTicketByIdQuery : IRequest<Ticket>
{
    public Guid TicketId { get; set; }

    public GetTicketByIdQuery(Guid ticketId)
    {
        TicketId = ticketId;
    }
}