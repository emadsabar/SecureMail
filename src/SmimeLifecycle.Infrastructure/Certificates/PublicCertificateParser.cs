using System.Security.Cryptography.X509Certificates;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Infrastructure.Certificates;

public sealed class PublicCertificateParser : IPublicCertificateParser
{
    public ParsedPublicCertificate ParsePem(string pem)
    {
        if (pem.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecretStorageException();
        }

        using var cert = X509Certificate2.CreateFromPem(pem);
        var exported = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
        var wrapped = "-----BEGIN CERTIFICATE-----\n" + exported + "\n-----END CERTIFICATE-----";
        return new ParsedPublicCertificate(
            cert.Thumbprint,
            cert.SerialNumber ?? string.Empty,
            cert.Subject,
            cert.Issuer,
            new DateTimeOffset(cert.NotBefore.ToUniversalTime(), TimeSpan.Zero),
            new DateTimeOffset(cert.NotAfter.ToUniversalTime(), TimeSpan.Zero),
            wrapped);
    }
}
