using SmimeLifecycle.Domain.Common;

namespace SmimeLifecycle.Domain.Entities;

public sealed class ManagedDevice : Entity
{
    public Guid DirectoryUserId { get; private set; }
    public string Hostname { get; private set; } = string.Empty;
    public string OsVersion { get; private set; } = string.Empty;
    public string AgentVersion { get; private set; } = string.Empty;
    /// <summary>Optional AD computer objectGUID when the device is domain-joined.</summary>
    public string? AdComputerObjectGuid { get; private set; }
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
        string? adComputerObjectGuid)
    {
        return new ManagedDevice
        {
            DirectoryUserId = directoryUserId,
            Hostname = hostname,
            OsVersion = osVersion,
            AgentVersion = agentVersion,
            EnrollmentSecretHash = enrollmentSecretHash,
            AdComputerObjectGuid = adComputerObjectGuid
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
