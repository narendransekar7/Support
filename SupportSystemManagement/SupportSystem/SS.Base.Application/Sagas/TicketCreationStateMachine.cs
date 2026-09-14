using MassTransit;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Sagas;

/// <summary>
/// Orchestrates the ticket-creation workflow: Assign Engineer -> Reserve SLA
/// on success, or Delete Ticket + failure email on the compensation path.
/// The "Send Email" (ticket-created) and "Create Notification" branches react
/// to TicketCreated directly and aren't gated by this saga (see
/// Consumers/CreateNotificationConsumer.cs and SS.Email.API's consumers).
/// </summary>
public class TicketCreationStateMachine : MassTransitStateMachine<TicketCreationSagaState>
{
    public State AssigningEngineer { get; private set; }
    public State ReservingSla { get; private set; }

    public Event<TicketCreated> TicketCreatedEvent { get; private set; }
    public Event<EngineerAssigned> EngineerAssignedEvent { get; private set; }
    public Event<AssignEngineerFailed> AssignEngineerFailedEvent { get; private set; }
    public Event<SlaReserved> SlaReservedEvent { get; private set; }

    public TicketCreationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TicketCreatedEvent, x => x.CorrelateById(m => m.Message.TicketId));
        Event(() => EngineerAssignedEvent, x => x.CorrelateById(m => m.Message.TicketId));
        Event(() => AssignEngineerFailedEvent, x => x.CorrelateById(m => m.Message.TicketId));
        Event(() => SlaReservedEvent, x => x.CorrelateById(m => m.Message.TicketId));

        Initially(
            When(TicketCreatedEvent)
                .Then(context =>
                {
                    context.Saga.TicketId = context.Message.TicketId;
                    context.Saga.Title = context.Message.Title;
                    context.Saga.Priority = context.Message.Priority;
                    context.Saga.CreatedByEmail = context.Message.CreatedByEmail;
                    context.Saga.CreatedByName = context.Message.CreatedByName;
                })
                .PublishAsync(context => context.Init<AssignEngineerCommand>(new AssignEngineerCommand
                {
                    TicketId = context.Message.TicketId
                }))
                .TransitionTo(AssigningEngineer)
        );

        During(AssigningEngineer,
            When(EngineerAssignedEvent)
                .PublishAsync(context => context.Init<ReserveSlaCommand>(new ReserveSlaCommand
                {
                    TicketId = context.Saga.TicketId,
                    Priority = context.Saga.Priority
                }))
                .TransitionTo(ReservingSla),
            When(AssignEngineerFailedEvent)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .PublishAsync(context => context.Init<DeleteTicketCommand>(new DeleteTicketCommand
                {
                    TicketId = context.Saga.TicketId
                }))
                .PublishAsync(context => context.Init<SendTicketCreationFailedEmail>(new SendTicketCreationFailedEmail
                {
                    TicketId = context.Saga.TicketId,
                    Title = context.Saga.Title,
                    CreatedByEmail = context.Saga.CreatedByEmail,
                    CreatedByName = context.Saga.CreatedByName,
                    Reason = context.Message.Reason
                }))
                .Finalize()
        );

        During(ReservingSla,
            When(SlaReservedEvent)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
