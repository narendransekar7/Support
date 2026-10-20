using System.Text.Json;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// Records any message as an <see cref="AuditLog"/> row, hosted by SS.Audit.API. Generic, so auditing
/// one more message type is one more Subscribe line in that host.
/// </summary>
public class AuditEventHandler<TMessage> : IEventHandler<TMessage> where TMessage : class
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditEventHandler<TMessage>> _logger;

    public AuditEventHandler(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork, ILogger<AuditEventHandler<TMessage>> logger)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(TMessage message, EventContext context, CancellationToken cancellationToken)
    {
        // EventId is unique in AuditLogs: a redelivered message is recorded once.
        if (await _auditLogRepository.ExistsByEventIdAsync(context.EventId, cancellationToken))
        {
            _logger.LogInformation("Message {EventId} ({MessageType}) already audited; skipping duplicate", context.EventId, typeof(TMessage).Name);
            return;
        }

        var entry = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            EventId = context.EventId,
            EventType = typeof(TMessage).Name,
            TicketId = (message as ITicketMessage)?.TicketId,
            UserId = (message as IUserMessage)?.UserId,
            ActorId = message is TicketCreated created ? created.CreatedBy : null,
            Provider = context.Provider,
            Payload = JsonSerializer.Serialize(message, Json),
            OccurredAtUtc = context.OccurredAtUtc,
            RecordedAtUtc = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Audited {MessageType} {EventId} for ticket {TicketId} / user {UserId} (via {Provider})",
            entry.EventType, entry.EventId, entry.TicketId, entry.UserId, entry.Provider);
    }
}
