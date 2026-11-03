using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

/// <summary>
/// Saga-state persistence for the provider-neutral ticket-creation orchestrator (Kafka and future
/// providers). With RabbitMQ, MassTransit's own EF saga repository uses the same table.
/// </summary>
public interface ITicketCreationSagaRepository : IGenericRepository<TicketCreationSagaState>
{
    // The saga's key is a Guid CorrelationId; messages carry the (int) TicketId, which is unique per saga.
    Task<TicketCreationSagaState?> GetByTicketIdAsync(int ticketId);
}
