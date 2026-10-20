using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.EventHandlers;

/// <summary>
/// Saga step "Reserve SLA": priority-based due dates, then the ticket is ready (InProgress).
/// Safe to repeat - a redelivery just recomputes the same dates.
/// </summary>
public class ReserveSlaHandler : IEventHandler<ReserveSlaCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<ReserveSlaHandler> _logger;

    public ReserveSlaHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork, IEventPublisher publisher, ILogger<ReserveSlaHandler> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task HandleAsync(ReserveSlaCommand message, EventContext context, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Reserve SLA skipped: ticket {TicketId} not found", message.TicketId);
            return;
        }

        var (response, resolution) = message.Priority switch
        {
            "High" => (TimeSpan.FromHours(4), TimeSpan.FromDays(1)),
            "Low" => (TimeSpan.FromDays(2), TimeSpan.FromDays(7)),
            _ => (TimeSpan.FromDays(1), TimeSpan.FromDays(3)) // Medium / default
        };

        ticket.ResponseDueDate = DateTime.Now.Add(response);
        ticket.ResolutionDueDate = DateTime.Now.Add(resolution);
        ticket.Status = TicketStatus.InProgress; // Ticket Ready

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Reserved {Priority} SLA for ticket {TicketId}: response due {ResponseDueDate}, resolution due {ResolutionDueDate}",
            message.Priority, ticket.TicketId, ticket.ResponseDueDate, ticket.ResolutionDueDate);

        await _publisher.PublishAsync(new SlaReserved { TicketId = ticket.TicketId }, ticket.TicketId.ToString(), cancellationToken);
    }
}
