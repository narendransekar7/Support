using MassTransit;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Consumers;

public class AssignEngineerConsumer : IConsumer<AssignEngineerCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignEngineerConsumer> _logger;

    public AssignEngineerConsumer(ITicketRepository ticketRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<AssignEngineerConsumer> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<AssignEngineerCommand> context)
    {
        var ticket = await _ticketRepository.GetByIdAsync(context.Message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Assign engineer skipped: ticket {TicketId} not found", context.Message.TicketId);
            return;
        }

        var engineer = await _userRepository.GetNextAgentForAssignmentAsync();
        if (engineer == null)
        {
            _logger.LogWarning("No engineer available for ticket {TicketId}; saga will compensate", ticket.TicketId);
            await context.Publish(new AssignEngineerFailed
            {
                TicketId = ticket.TicketId,
                Reason = "No agents are available to assign."
            });
            return;
        }

        ticket.AssignedTo = engineer.UserId;
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Assigned engineer {EngineerId} to ticket {TicketId}", engineer.UserId, ticket.TicketId);

        await context.Publish(new EngineerAssigned
        {
            TicketId = ticket.TicketId,
            EngineerId = engineer.UserId
        });
    }
}
