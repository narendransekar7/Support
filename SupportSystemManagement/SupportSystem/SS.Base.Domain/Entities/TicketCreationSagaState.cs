using MassTransit;

namespace SS.Base.Domain.Entities;

/// <summary>
/// Persisted saga state for the ticket-creation workflow (see
/// SS.Base.Application/Sagas/TicketCreationStateMachine.cs). CorrelationId is
/// the ticket's TicketId.
/// </summary>
public class TicketCreationSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid TicketId { get; set; }
    public string? Title { get; set; }
    public string? Priority { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? CreatedByName { get; set; }
    public string? FailureReason { get; set; }
    public byte[] RowVersion { get; set; }
}
