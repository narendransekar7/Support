using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// Compensation step: removes a ticket that could not be assigned an engineer.
/// </summary>
public class DeleteTicketHandler : IEventHandler<DeleteTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTicketHandler> _logger;

    public DeleteTicketHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork, ILogger<DeleteTicketHandler> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteTicketCommand message, EventContext context, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Compensation: ticket {TicketId} already gone, nothing to delete", message.TicketId);
            return;
        }

        await _ticketRepository.RemoveAsync(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Compensation: deleted ticket {TicketId}", ticket.TicketId);
    }
}
