using Microsoft.Extensions.Logging;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// Saga step "Assign Engineer": round-robins over the agents. Called by AssignEngineerConsumer
/// (RabbitMQ) or a Kafka consumer worker - the logic is the same for every broker.
/// </summary>
public class AssignEngineerHandler : IEventHandler<AssignEngineerCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<AssignEngineerHandler> _logger;

    public AssignEngineerHandler(ITicketRepository ticketRepository, IUserRepository userRepository, IUnitOfWork unitOfWork,
        IEventPublisher publisher, ILogger<AssignEngineerHandler> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task HandleAsync(AssignEngineerCommand message, EventContext context, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Assign engineer skipped: ticket {TicketId} not found", message.TicketId);
            return;
        }

        // Redelivered command (at-least-once): the engineer was already assigned, so just report it
        // again instead of moving the round-robin cursor a second time.
        if (ticket.AssignedTo is Guid alreadyAssigned)
        {
            _logger.LogInformation("Ticket {TicketId} already has engineer {EngineerId}; re-publishing EngineerAssigned", ticket.TicketId, alreadyAssigned);
            await _publisher.PublishAsync(new EngineerAssigned { TicketId = ticket.TicketId, EngineerId = alreadyAssigned },
                ticket.TicketId.ToString(), cancellationToken);
            return;
        }

        var engineer = await _userRepository.GetNextAgentForAssignmentAsync();
        if (engineer == null)
        {
            _logger.LogWarning("No engineer available for ticket {TicketId}; saga will compensate", ticket.TicketId);
            await _publisher.PublishAsync(new AssignEngineerFailed
            {
                TicketId = ticket.TicketId,
                Reason = "No agents are available to assign."
            }, ticket.TicketId.ToString(), cancellationToken);
            return;
        }

        ticket.AssignedTo = engineer.UserId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Assigned engineer {EngineerId} to ticket {TicketId}", engineer.UserId, ticket.TicketId);

        await _publisher.PublishAsync(new EngineerAssigned
        {
            TicketId = ticket.TicketId,
            EngineerId = engineer.UserId
        }, ticket.TicketId.ToString(), cancellationToken);
    }
}
