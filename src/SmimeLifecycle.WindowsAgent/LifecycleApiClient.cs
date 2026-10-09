using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace SmimeLifecycle.WindowsAgent;

public sealed class LifecycleApiClient(HttpClient http, IOptions<AgentOptions> options)
{
    public async Task<Guid> RegisterAsync(object body, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.PostAsJsonAsync("api/agent/devices", body, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
    }

    public async Task<PendingEnrollment?> GetPendingAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.GetAsync($"api/agent/devices/{deviceId}/pending-enrollment", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PendingEnrollment>(cancellationToken: cancellationToken);
    }

    public async Task SubmitCsrAsync(object body, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.PostAsJsonAsync("api/agent/csr", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IssuedPublicCert?> GetIssuedAsync(Guid enrollmentId, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.GetAsync($"api/agent/enrollments/{enrollmentId}/certificate", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IssuedPublicCert>(cancellationToken: cancellationToken);
    }

    public async Task ReportInstalledAsync(object body, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.PostAsJsonAsync("api/agent/installed", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReportOutlookAsync(object body, CancellationToken cancellationToken)
    {
        EnsureBase();
        using var response = await http.PostAsJsonAsync("api/agent/outlook-check", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private void EnsureBase()
    {
        http.BaseAddress ??= new Uri(options.Value.ApiBaseUrl.TrimEnd('/') + "/");
    }
}

public sealed record PendingEnrollment(Guid EnrollmentId, Guid DirectoryUserId, string Subject, string Mail);

public sealed record IssuedPublicCert(Guid CertificateId, string Thumbprint, string PublicCertificatePem);
