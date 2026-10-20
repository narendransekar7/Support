namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;

/// <summary>Newest audit entries first, optionally for one ticket or one user.</summary>
public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
{
    public Guid? TicketId { get; set; }
    public Guid? UserId { get; set; }
    public int Take { get; set; }

    public GetAuditLogsQuery(Guid? ticketId, int take, Guid? userId = null)
    {
        TicketId = ticketId;
        UserId = userId;
        Take = take;
    }
}
