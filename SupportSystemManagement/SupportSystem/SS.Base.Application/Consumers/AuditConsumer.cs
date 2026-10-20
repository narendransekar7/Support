using MassTransit;
using SS.Base.Application.EventHandlers;
using SS.Base.Messaging.RabbitMq;

namespace SS.Base.Application.Consumers;

/// <summary>
/// RabbitMQ (MassTransit) entry point of the audit trail, hosted by SS.Audit.API; the logic lives in
/// <see cref="AuditEventHandler{TMessage}"/>. Register one closed type per audited message, with an
/// explicit endpoint name (generic type names make poor queue names).
/// </summary>
public class AuditConsumer<TMessage> : IConsumer<TMessage> where TMessage : class
{
    private readonly AuditEventHandler<TMessage> _handler;

    public AuditConsumer(AuditEventHandler<TMessage> handler) => _handler = handler;

    public Task Consume(ConsumeContext<TMessage> context) =>
        _handler.HandleAsync(context.Message, context.ToEventContext(), context.CancellationToken);
}
