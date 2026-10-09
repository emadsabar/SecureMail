namespace SmimeLifecycle.WindowsAgent;

/// <summary>
/// Endpoint agent options. The agent talks only to the Certificate Management Server API —
/// never to the certificate authority.
/// </summary>
public sealed class AgentOptions
{
    public const string SectionName = "Agent";
    public string ApiBaseUrl { get; set; } = "http://localhost:5088";
    public Guid DirectoryUserId { get; set; }
    public string EnrollmentSecret { get; set; } = "dev-enrollment-secret";
    public string? AdComputerObjectGuid { get; set; }
    public int PollSeconds { get; set; } = 15;
}
