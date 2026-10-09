namespace SmimeLifecycle.Application.Abstractions;

public sealed record ParsedPublicCertificate(
    string Thumbprint,
    string SerialNumber,
    string Subject,
    string Issuer,
    DateTimeOffset NotBefore,
    DateTimeOffset NotAfter,
    string Pem);

public interface IPublicCertificateParser
{
    ParsedPublicCertificate ParsePem(string pem);
}
