namespace SS.Base.Application.Queries;
using MediatR;
using SS.Base.Domain.Entities;
using SS.Base.Domain.Interfaces.Repository;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, IEnumerable<AuditLog>>
{
    private const int MaxTake = 500;

    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IEnumerable<AuditLog>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return await _auditLogRepository.GetRecentAsync(request.TicketId, Math.Clamp(request.Take, 1, MaxTake), cancellationToken);
    }
}
