using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Infrastructure.Persistence;

public sealed class AuditService(ApplicationDbContext db) : IAuditService
{
    public async Task WriteAsync(
        AuditActorType actorType,
        string actor,
        string action,
        string entityType,
        Guid? entityId,
        string details,
        CancellationToken cancellationToken)
    {
        db.AuditEvents.Add(AuditEvent.Record(actorType, actor, action, entityType, entityId, details));
        await db.SaveChangesAsync(cancellationToken);
    }
}
