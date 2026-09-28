using MassTransit;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Compensation step: removes a ticket that could not be assigned an engineer.
/// </summary>
public class DeleteTicketConsumer : IConsumer<DeleteTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTicketConsumer> _logger;

    public DeleteTicketConsumer(ITicketRepository ticketRepository, IUnitOfWork unitOfWork, ILogger<DeleteTicketConsumer> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<DeleteTicketCommand> context)
    {
        var ticket = await _ticketRepository.GetByIdAsync(context.Message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Compensation: ticket {TicketId} already gone, nothing to delete", context.Message.TicketId);
            return;
        }

        await _ticketRepository.RemoveAsync(ticket);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogWarning("Compensation: deleted ticket {TicketId}", ticket.TicketId);
    }
}
