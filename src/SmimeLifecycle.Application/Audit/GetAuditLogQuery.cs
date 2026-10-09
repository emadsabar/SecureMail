using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Audit;

public sealed record AuditLogItem(
    Guid Id,
    DateTimeOffset CreatedAt,
    AuditActorType ActorType,
    string Actor,
    string Action,
    string EntityType,
    Guid? EntityId,
    string Details);

public sealed record GetAuditLogQuery(int Take = 200) : IRequest<IReadOnlyList<AuditLogItem>>;

public sealed class GetAuditLogQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAuditLogQuery, IReadOnlyList<AuditLogItem>>
{
    public async Task<IReadOnlyList<AuditLogItem>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        return await db.AuditEvents
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.Take)
            .Select(a => new AuditLogItem(a.Id, a.CreatedAt, a.ActorType, a.Actor, a.Action, a.EntityType, a.EntityId, a.Details))
            .ToListAsync(cancellationToken);
    }
}
