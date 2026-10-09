using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmimeLifecycle.Application.Abstractions;

namespace SmimeLifecycle.Infrastructure.CertificateAuthority;

public sealed class SectigoOptions
{
    public const string SectionName = "CertificateAuthority:Sectigo";

    /// <summary>
    /// Base URL supplied by Sectigo Certificate Manager documentation for your tenant.
    /// Do not invent a hostname.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Relative path template from Sectigo documentation, e.g. the documented enrollment resource.
    /// Leave empty until the official path is configured.
    /// </summary>
    public string? EnrollmentPath { get; set; }

    public string? StatusPathTemplate { get; set; }

    public string? RevocationPathTemplate { get; set; }

    /// <summary>
    /// Authentication scheme and credential material must match Sectigo documentation.
    /// Resolve secrets from Key Vault; never persist them in PostgreSQL.
    /// </summary>
    public string? AuthorizationHeaderScheme { get; set; }

    public string? CredentialSecretName { get; set; }
}

/// <summary>
/// Sectigo adapter. HTTP paths, hostnames, and auth formats are configuration-only and must
/// come from Sectigo Certificate Manager documentation for the customer tenant.
/// </summary>
public sealed class SectigoCertificateAuthority(HttpClient http, IOptions<SectigoOptions> options) : ICertificateAuthority
{
    public async Task<string> SubmitEnrollmentAsync(CertificateAuthorityRequest request, CancellationToken cancellationToken)
    {
        var o = RequireConfigured();
        using var message = new HttpRequestMessage(HttpMethod.Post, o.EnrollmentPath)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    csr = request.CsrPem,
                    subject = request.Subject,
                    email = request.Email,
                    profile = request.ProfileParameters
                }),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await http.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!TryReadId(doc.RootElement, out var id))
        {
            throw new InvalidOperationException("Sectigo enrollment response did not contain a request identifier. Map the documented JSON field in configuration/code after reviewing the official API.");
        }

        return id;
    }

    public async Task<CertificateAuthorityStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken)
    {
        var o = RequireConfigured();
        var path = (o.StatusPathTemplate ?? string.Empty).Replace("{requestId}", Uri.EscapeDataString(requestId), StringComparison.Ordinal);
        using var response = await http.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = doc.RootElement;
        var stateText = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : "pending";
        var pem = root.TryGetProperty("certificatePem", out var pemEl) ? pemEl.GetString() : null;
        var state = MapState(stateText);
        return new CertificateAuthorityStatus(requestId, state, pem, stateText);
    }

    public async Task RevokeAsync(string requestId, string reason, CancellationToken cancellationToken)
    {
        var o = RequireConfigured();
        var path = (o.RevocationPathTemplate ?? string.Empty).Replace("{requestId}", Uri.EscapeDataString(requestId), StringComparison.Ordinal);
        using var content = new StringContent(JsonSerializer.Serialize(new { reason }), Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private SectigoOptions RequireConfigured()
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.BaseUrl) ||
            string.IsNullOrWhiteSpace(o.EnrollmentPath) ||
            string.IsNullOrWhiteSpace(o.StatusPathTemplate) ||
            string.IsNullOrWhiteSpace(o.RevocationPathTemplate))
        {
            throw new InvalidOperationException(
                "Sectigo Certificate Manager is not configured. Set CertificateAuthority:Sectigo BaseUrl and path templates from official Sectigo documentation. Do not guess API routes.");
        }

        if (http.BaseAddress is null)
        {
            http.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
        }

        if (!string.IsNullOrWhiteSpace(o.AuthorizationHeaderScheme) && http.DefaultRequestHeaders.Authorization is null)
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(o.AuthorizationHeaderScheme, "configure-from-key-vault");
        }

        return o;
    }

    private static bool TryReadId(JsonElement root, out string id)
    {
        foreach (var name in new[] { "id", "requestId", "orderNumber", "ref" })
        {
            if (root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String && el.GetString() is { Length: > 0 } value)
            {
                id = value;
                return true;
            }
        }

        id = string.Empty;
        return false;
    }

    private static CertificateAuthorityRequestState MapState(string? status) =>
        status?.ToLowerInvariant() switch
        {
            "issued" or "complete" or "completed" => CertificateAuthorityRequestState.Issued,
            "rejected" or "failed" or "denied" => CertificateAuthorityRequestState.Rejected,
            "revoked" => CertificateAuthorityRequestState.Revoked,
            "submitted" => CertificateAuthorityRequestState.Submitted,
            _ => CertificateAuthorityRequestState.Pending
        };
}
