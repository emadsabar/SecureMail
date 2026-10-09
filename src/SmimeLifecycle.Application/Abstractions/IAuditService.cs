using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Abstractions;

public interface IAuditService
{
    Task WriteAsync(
        AuditActorType actorType,
        string actor,
        string action,
        string entityType,
        Guid? entityId,
        string details,
        CancellationToken cancellationToken);
}
