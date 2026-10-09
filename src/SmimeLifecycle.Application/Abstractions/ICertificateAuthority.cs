namespace SmimeLifecycle.Application.Abstractions;

/// <summary>
/// Outbound CA integration used exclusively by the Certificate Management Server.
/// Windows agents, Outlook add-ins, and browsers must never call the CA; they call the CMS API only.
/// Implementations live in SmimeLifecycle.Infrastructure and are registered only on the CMS host.
/// </summary>
public sealed record CertificateAuthorityRequest(
    string CsrPem,
    string Subject,
    string Email,
    IReadOnlyDictionary<string, string> ProfileParameters);

public sealed record CertificateAuthorityStatus(
    string RequestId,
    CertificateAuthorityRequestState State,
    string? PublicCertificatePem,
    string? Detail);

public enum CertificateAuthorityRequestState
{
    Submitted = 0,
    Pending = 1,
    Issued = 2,
    Rejected = 3,
    Revoked = 4
}

public interface ICertificateAuthority
{
    Task<string> SubmitEnrollmentAsync(CertificateAuthorityRequest request, CancellationToken cancellationToken);

    Task<CertificateAuthorityStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken);

    Task RevokeAsync(string requestId, string reason, CancellationToken cancellationToken);
}
