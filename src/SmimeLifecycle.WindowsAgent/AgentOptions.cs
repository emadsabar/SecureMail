namespace SmimeLifecycle.WindowsAgent;

public sealed class AgentOptions
{
    public const string SectionName = "Agent";
    public string ApiBaseUrl { get; set; } = "http://localhost:5088";
    public Guid DirectoryUserId { get; set; }
    public string EnrollmentSecret { get; set; } = "dev-enrollment-secret";
    public string? EntraDeviceId { get; set; }
    public int PollSeconds { get; set; } = 15;
}
