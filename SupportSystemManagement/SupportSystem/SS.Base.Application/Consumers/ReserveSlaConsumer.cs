using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// Saga step "Reserve SLA".
/// RabbitMQ (MassTransit) entry point; the logic lives in <see cref="ReserveSlaHandler"/>, which the Kafka
/// provider calls directly. Keeping this class keeps its RabbitMQ queue name ("ReserveSla").
/// </summary>
public class ReserveSlaConsumer : IConsumer<ReserveSlaCommand>
{
    private readonly ReserveSlaHandler _handler;

    public ReserveSlaConsumer(ReserveSlaHandler handler) => _handler = handler;

    public Task Consume(ConsumeContext<ReserveSlaCommand> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
