using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SmimeLifecycle.Application.Abstractions;

namespace SmimeLifecycle.Infrastructure.CertificateAuthority;

/// <summary>
/// Local development CA that signs submitted CSRs. Not for production and not a Sectigo stand-in.
/// </summary>
public sealed class DevelopmentCertificateAuthority : ICertificateAuthority
{
    private readonly ConcurrentDictionary<string, CertificateAuthorityStatus> _requests = new();
    private readonly X509Certificate2 _issuer;

    public DevelopmentCertificateAuthority()
    {
        _issuer = CreateIssuer();
    }

    public Task<string> SubmitEnrollmentAsync(CertificateAuthorityRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString("N");
        var issued = SignCsr(request.CsrPem);
        _requests[id] = new CertificateAuthorityStatus(id, CertificateAuthorityRequestState.Issued, issued, "Development CA issued a lab certificate.");
        return Task.FromResult(id);
    }

    public Task<CertificateAuthorityStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken)
    {
        if (!_requests.TryGetValue(requestId, out var status))
        {
            status = new CertificateAuthorityStatus(requestId, CertificateAuthorityRequestState.Rejected, null, "Unknown request.");
        }

        return Task.FromResult(status);
    }

    public Task RevokeAsync(string requestId, string reason, CancellationToken cancellationToken)
    {
        _requests[requestId] = new CertificateAuthorityStatus(requestId, CertificateAuthorityRequestState.Revoked, null, reason);
        return Task.CompletedTask;
    }

    private string SignCsr(string csrPem)
    {
        var csr = CertificateRequest.LoadSigningRequestPem(
            csrPem,
            HashAlgorithmName.SHA256,
            CertificateRequestLoadOptions.Default);
        var notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
        var notAfter = DateTimeOffset.UtcNow.AddYears(1);
        using var issued = csr.Create(_issuer, notBefore, notAfter, BitConverter.GetBytes(RandomNumberGenerator.GetInt32(int.MaxValue)));
        var b64 = Convert.ToBase64String(issued.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN CERTIFICATE-----\n{b64}\n-----END CERTIFICATE-----";
    }

    private static X509Certificate2 CreateIssuer()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=SmimeLifecycle Development CA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5));
    }
}
