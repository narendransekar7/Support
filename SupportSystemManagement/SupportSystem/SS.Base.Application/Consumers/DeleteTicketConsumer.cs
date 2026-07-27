using MassTransit;
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

    public DeleteTicketConsumer(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<DeleteTicketCommand> context)
    {
        var ticket = await _ticketRepository.GetByIdAsync(context.Message.TicketId);
        if (ticket == null)
        {
            return;
        }

        await _ticketRepository.RemoveAsync(ticket);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
