using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Saga step "Assign Engineer".
/// RabbitMQ (MassTransit) entry point; the logic lives in <see cref="AssignEngineerHandler"/>, which the Kafka
/// provider calls directly. Keeping this class keeps its RabbitMQ queue name ("AssignEngineer").
/// </summary>
public class AssignEngineerConsumer : IConsumer<AssignEngineerCommand>
{
    private readonly AssignEngineerHandler _handler;

    public AssignEngineerConsumer(AssignEngineerHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<AssignEngineerCommand> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
