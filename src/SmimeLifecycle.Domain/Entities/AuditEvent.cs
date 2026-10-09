using SmimeLifecycle.Domain.Common;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Domain.Entities;

public sealed class AuditEvent : Entity
{
    public AuditActorType ActorType { get; private set; }
    public string Actor { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string Details { get; private set; } = string.Empty;

    private AuditEvent()
    {
    }

    public static AuditEvent Record(
        AuditActorType actorType,
        string actor,
        string action,
        string entityType,
        Guid? entityId,
        string details)
    {
        return new AuditEvent
        {
            ActorType = actorType,
            Actor = actor,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = Sanitize(details)
        };
    }

    private static string Sanitize(string details)
    {
        var blocked = new[] { "PRIVATE KEY", "password", "pfx", "client_secret", "access_token", "refresh_token" };
        var sanitized = details;
        foreach (var token in blocked)
        {
            if (sanitized.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                sanitized = "[redacted: potential secret material omitted from audit payload]";
                break;
            }
        }

        return sanitized;
    }
}
