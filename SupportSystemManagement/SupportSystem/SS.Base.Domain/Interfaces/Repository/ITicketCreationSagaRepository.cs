using SS.Base.Domain.Entities;

namespace SS.Base.Domain.Interfaces.Repository;

/// <summary>
/// Saga-state persistence for the provider-neutral ticket-creation orchestrator (Kafka and future
/// providers). With RabbitMQ, MassTransit's own EF saga repository uses the same table.
/// </summary>
public interface ITicketCreationSagaRepository : IGenericRepository<TicketCreationSagaState>
{
}
