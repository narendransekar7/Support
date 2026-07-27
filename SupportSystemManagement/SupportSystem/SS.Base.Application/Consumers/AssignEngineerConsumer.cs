using MassTransit;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Consumers;

public class AssignEngineerConsumer : IConsumer<AssignEngineerCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignEngineerConsumer(ITicketRepository ticketRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<AssignEngineerCommand> context)
    {
        var ticket = await _ticketRepository.GetByIdAsync(context.Message.TicketId);
        if (ticket == null)
        {
            return;
        }

        var engineer = await _userRepository.GetNextAgentForAssignmentAsync();
        if (engineer == null)
        {
            await context.Publish(new AssignEngineerFailed
            {
                TicketId = ticket.TicketId,
                Reason = "No agents are available to assign."
            });
            return;
        }

        ticket.AssignedTo = engineer.UserId;
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        await context.Publish(new EngineerAssigned
        {
            TicketId = ticket.TicketId,
            EngineerId = engineer.UserId
        });
    }
}
