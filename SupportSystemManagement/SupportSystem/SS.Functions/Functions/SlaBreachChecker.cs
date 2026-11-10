using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SS.Base.Domain.Interfaces.Messaging;
using SS.Base.Domain.Interfaces.Repository;
using SS.Base.Domain.Messages.Ticket;

namespace SS.Functions;

/// <summary>
/// Timer-triggered: finds tickets that are still open after their resolution due date (set by the ticket
/// saga's "Reserve SLA" step) and publishes <see cref="TicketSlaBreached"/> once for each.
/// SS.Notification.API notifies the assignee and SS.Audit.API records it.
///
/// The functions host creates this class per invocation inside a DI scope, so scoped services
/// (the DbContext behind the repositories) are fresh for every run.
/// </summary>
public class SlaBreachChecker
{
    private readonly ITicketRepository _tickets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;
    private readonly SlaBreachCheckerOptions _options;
    private readonly ILogger<SlaBreachChecker> _logger;

    public SlaBreachChecker(ITicketRepository tickets, IUnitOfWork unitOfWork, IEventPublisher publisher,
        IOptions<SlaBreachCheckerOptions> options, ILogger<SlaBreachChecker> logger)
    {
        _tickets = tickets;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
    }

    // NCRONTAB with seconds: "{second} {minute} {hour} {day} {month} {day-of-week}". %...% reads the schedule from
    // the SlaBreachCheckerSchedule app setting, so it can change per environment without a rebuild
    // ("0 */5 * * * *" = every 5 minutes; "*/30 * * * * *" = every 30 s, handy while trying it out).
    // Run it on demand locally: POST http://localhost:7071/admin/functions/SlaBreachChecker with body {}.
    [Function(nameof(SlaBreachChecker))]
    public async Task Run([TimerTrigger("%SlaBreachCheckerSchedule%")] TimerInfo timer, CancellationToken cancellationToken)
    {
        if (timer.IsPastDue)
            _logger.LogWarning("SLA breach check is running late (the app was stopped or busy when it was due)");

        // Same clock as ReserveSlaHandler, which sets the due dates with DateTime.Now. In containers and on
        // Azure the process time zone is UTC, so this is UTC there.
        var now = DateTime.Now;

        var breached = await _tickets.GetUnreportedSlaBreachesAsync(now, _options.BatchSize, cancellationToken);
        if (breached.Count == 0)
        {
            _logger.LogInformation("SLA breach check: no new breaches");
            return;
        }

        var reported = 0;
        foreach (var ticket in breached)
        {
            // Publish first, then mark the ticket: if the run dies in between, the next run publishes again
            // (at-least-once) instead of the breach never being reported.
            await _publisher.PublishAsync(new TicketSlaBreached
            {
                TicketId = ticket.TicketId,
                Title = ticket.Title,
                Priority = ticket.Priority,
                ResolutionDueDate = ticket.ResolutionDueDate!.Value,
                DetectedAt = now,
                CreatedBy = ticket.CreatedBy,
                AssignedTo = ticket.AssignedTo
            }, ticket.TicketId.ToString(), cancellationToken);

            ticket.SlaBreachedAt = now;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            reported++;

            _logger.LogInformation("Ticket {TicketId} ({Priority}) breached its SLA: resolution was due {ResolutionDueDate}",
                ticket.TicketId, ticket.Priority, ticket.ResolutionDueDate);
        }

        _logger.LogInformation("SLA breach check: reported {Count} ticket(s){More}", reported,
            breached.Count == _options.BatchSize ? "; more remain for the next run" : "");
    }
}
