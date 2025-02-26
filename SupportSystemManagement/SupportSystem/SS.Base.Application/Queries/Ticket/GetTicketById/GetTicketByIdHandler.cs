namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

public class GetTicketByIdHandler : IRequestHandler<GetTicketByIdQuery, Ticket>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithUpdatesAsync(request.TicketId);

        if (ticket == null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        return ticket;
    }
}