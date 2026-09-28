using MassTransit;
using Microsoft.Extensions.Logging;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Base.Application.Consumers;

public class ReserveSlaConsumer : IConsumer<ReserveSlaCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReserveSlaConsumer> _logger;

    public ReserveSlaConsumer(ITicketRepository ticketRepository, IUnitOfWork unitOfWork, ILogger<ReserveSlaConsumer> logger)
    {
        _logger = logger;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<ReserveSlaCommand> context)
    {
        var ticket = await _ticketRepository.GetByIdAsync(context.Message.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Reserve SLA skipped: ticket {TicketId} not found", context.Message.TicketId);
            return;
        }

        var (response, resolution) = context.Message.Priority switch
        {
            "High" => (TimeSpan.FromHours(4), TimeSpan.FromDays(1)),
            "Low" => (TimeSpan.FromDays(2), TimeSpan.FromDays(7)),
            _ => (TimeSpan.FromDays(1), TimeSpan.FromDays(3)) // Medium / default
        };

        ticket.ResponseDueDate = DateTime.Now.Add(response);
        ticket.ResolutionDueDate = DateTime.Now.Add(resolution);
        ticket.Status = TicketStatus.InProgress; // Ticket Ready

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Reserved {Priority} SLA for ticket {TicketId}: response due {ResponseDueDate}, resolution due {ResolutionDueDate}",
            context.Message.Priority, ticket.TicketId, ticket.ResponseDueDate, ticket.ResolutionDueDate);

        await context.Publish(new SlaReserved { TicketId = ticket.TicketId });
    }
}
