namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;

/// <summary>Newest audit entries first, optionally for one ticket.</summary>
public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
{
    public Guid? TicketId { get; set; }
    public int Take { get; set; }

    public GetAuditLogsQuery(Guid? ticketId, int take)
    {
        TicketId = ticketId;
        Take = take;
    }
}
