using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Sagas;

/// <summary>
/// The ticket-creation saga for brokers without MassTransit (Kafka now, Azure Service Bus later):
/// Assign Engineer -> Reserve SLA, or Delete Ticket + failure email on the compensation path.
/// It is the same workflow as <see cref="TicketCreationStateMachine"/> (which RabbitMQ keeps using),
/// written as plain handlers so the moving parts of an orchestrated saga are visible:
///
///   TicketCreated        (no state)          -> save AssigningEngineer, send AssignEngineerCommand
///   EngineerAssigned     AssigningEngineer   -> save ReservingSla,      send ReserveSlaCommand
///   AssignEngineerFailed AssigningEngineer   -> send DeleteTicketCommand + SendTicketCreationFailedEmail, finish
///   SlaReserved          ReservingSla        -> finish
///
/// State lives in the same TicketCreationSagaStates table (CorrelationId = TicketId, same state
/// names); a finished saga's row is deleted, like MassTransit's SetCompletedWhenFinalized.
///
/// At-least-once delivery means any of these can arrive twice, and a crash can happen between
/// saving state and publishing the next command. So a message that finds the saga already in the
/// state it would move to re-sends that state's command (the steps are idempotent), and anything
/// else unexpected is ignored. RowVersion turns two concurrent updates into a retry.
/// </summary>
public class TicketCreationOrchestrator :
    IEventHandler<TicketCreated>,
    IEventHandler<EngineerAssigned>,
    IEventHandler<AssignEngineerFailed>,
    IEventHandler<SlaReserved>
{
    public const string AssigningEngineer = nameof(AssigningEngineer);
    public const string ReservingSla = nameof(ReservingSla);

    private readonly ITicketCreationSagaRepository _sagaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<TicketCreationOrchestrator> _logger;

    public TicketCreationOrchestrator(ITicketCreationSagaRepository sagaRepository, IUnitOfWork unitOfWork, IEventPublisher publisher,
        ILogger<TicketCreationOrchestrator> logger)
    {
        _sagaRepository = sagaRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(TicketCreated message, EventContext context, CancellationToken cancellationToken)
    {
        var saga = await _sagaRepository.GetByIdAsync(message.TicketId);
        if (saga == null)
        {
            saga = new TicketCreationSagaState
            {
                CorrelationId = message.TicketId,
                TicketId = message.TicketId,
                Title = message.Title,
                Priority = message.Priority,
                CreatedByEmail = message.CreatedByEmail,
                CreatedByName = message.CreatedByName,
                CurrentState = AssigningEngineer
            };
            await _sagaRepository.AddAsync(saga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ticket-creation saga started for ticket {TicketId}", message.TicketId);
        }
        else if (saga.CurrentState != AssigningEngineer)
        {
            _logger.LogInformation("Saga for ticket {TicketId} is already {State}; ignoring duplicate TicketCreated", saga.TicketId, saga.CurrentState);
            return;
        }

        await SendAssignEngineer(saga, cancellationToken);
    }

    public async Task HandleAsync(EngineerAssigned message, EventContext context, CancellationToken cancellationToken)
    {
        var saga = await _sagaRepository.GetByIdAsync(message.TicketId);
        if (saga == null || (saga.CurrentState != AssigningEngineer && saga.CurrentState != ReservingSla))
        {
            _logger.LogInformation("No saga waiting for an engineer on ticket {TicketId} ({State}); ignoring EngineerAssigned",
                message.TicketId, saga?.CurrentState ?? "finished");
            return;
        }

        if (saga.CurrentState == AssigningEngineer)
        {
            _logger.LogInformation("Saga: engineer {EngineerId} assigned to ticket {TicketId}, reserving SLA", message.EngineerId, saga.TicketId);
            saga.CurrentState = ReservingSla;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await SendReserveSla(saga, cancellationToken);
    }

    public async Task HandleAsync(AssignEngineerFailed message, EventContext context, CancellationToken cancellationToken)
    {
        var saga = await _sagaRepository.GetByIdAsync(message.TicketId);
        if (saga == null || saga.CurrentState != AssigningEngineer)
        {
            _logger.LogInformation("No saga waiting for an engineer on ticket {TicketId} ({State}); ignoring AssignEngineerFailed",
                message.TicketId, saga?.CurrentState ?? "finished");
            return;
        }

        _logger.LogWarning("Saga: assigning an engineer to ticket {TicketId} failed ({Reason}); compensating", saga.TicketId, message.Reason);

        // Publish the compensations first, then finish: if we crash in between, the redelivered
        // AssignEngineerFailed sends them again (deleting is idempotent) rather than never.
        await _publisher.PublishAsync(new DeleteTicketCommand { TicketId = saga.TicketId }, saga.TicketId.ToString(), cancellationToken);
        await _publisher.PublishAsync(new SendTicketCreationFailedEmail
        {
            TicketId = saga.TicketId,
            Title = saga.Title,
            CreatedByEmail = saga.CreatedByEmail,
            CreatedByName = saga.CreatedByName,
            Reason = message.Reason
        }, saga.TicketId.ToString(), cancellationToken);

        await _sagaRepository.RemoveAsync(saga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(SlaReserved message, EventContext context, CancellationToken cancellationToken)
    {
        var saga = await _sagaRepository.GetByIdAsync(message.TicketId);
        if (saga == null || saga.CurrentState != ReservingSla)
        {
            _logger.LogInformation("No saga reserving an SLA for ticket {TicketId} ({State}); ignoring SlaReserved",
                message.TicketId, saga?.CurrentState ?? "finished");
            return;
        }

        await _sagaRepository.RemoveAsync(saga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ticket-creation saga completed for ticket {TicketId}", saga.TicketId);
    }

    private Task SendAssignEngineer(TicketCreationSagaState saga, CancellationToken cancellationToken) =>
        _publisher.PublishAsync(new AssignEngineerCommand { TicketId = saga.TicketId }, saga.TicketId.ToString(), cancellationToken);

    private Task SendReserveSla(TicketCreationSagaState saga, CancellationToken cancellationToken) =>
        _publisher.PublishAsync(new ReserveSlaCommand { TicketId = saga.TicketId, Priority = saga.Priority }, saga.TicketId.ToString(), cancellationToken);
}
