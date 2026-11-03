namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;

/// <summary>Newest audit entries first, optionally for one ticket or one user.</summary>
public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
{
    public int? TicketId { get; set; }
    public int? UserId { get; set; }
    public int Take { get; set; }

    public GetAuditLogsQuery(int? ticketId, int take, int? userId = null)
    {
        TicketId = ticketId;
        UserId = userId;
        Take = take;
    }
}
