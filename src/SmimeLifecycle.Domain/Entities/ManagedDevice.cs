using SmimeLifecycle.Domain.Common;

namespace SmimeLifecycle.Domain.Entities;

public sealed class ManagedDevice : Entity
{
    public Guid DirectoryUserId { get; private set; }
    public string Hostname { get; private set; } = string.Empty;
    public string OsVersion { get; private set; } = string.Empty;
    public string AgentVersion { get; private set; } = string.Empty;
    public string? EntraDeviceId { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; } = DateTimeOffset.UtcNow;
    public string EnrollmentSecretHash { get; private set; } = string.Empty;

    private ManagedDevice()
    {
    }

    public static ManagedDevice Register(
        Guid directoryUserId,
        string hostname,
        string osVersion,
        string agentVersion,
        string enrollmentSecretHash,
        string? entraDeviceId)
    {
        return new ManagedDevice
        {
            DirectoryUserId = directoryUserId,
            Hostname = hostname,
            OsVersion = osVersion,
            AgentVersion = agentVersion,
            EnrollmentSecretHash = enrollmentSecretHash,
            EntraDeviceId = entraDeviceId
        };
    }

    public void Heartbeat(string agentVersion, string osVersion)
    {
        AgentVersion = agentVersion;
        OsVersion = osVersion;
        LastSeenAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
