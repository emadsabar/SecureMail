using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Options;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Infrastructure.Directory;

public sealed class ActiveDirectoryOptions
{
    public const string SectionName = "ActiveDirectory";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 636;
    public string BindUserSecretName { get; set; } = "ad-bind-user";
    public string BindPasswordSecretName { get; set; } = "ad-bind-password";
    public string? BindUser { get; set; }
    public string? BindPassword { get; set; }
    public bool UseSsl { get; set; } = true;
}

public sealed class LdapsActiveDirectoryPublisher(IOptions<ActiveDirectoryOptions> options) : IActiveDirectoryPublisher
{
    public Task PublishPublicCertificateAsync(
        string distinguishedName,
        byte[] derEncodedCertificate,
        DirectoryAttribute attribute,
        CancellationToken cancellationToken)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.BindUser) || string.IsNullOrWhiteSpace(o.BindPassword))
        {
            throw new InvalidOperationException(
                "AD bind credentials are not configured. Store them in Key Vault and map BindUser/BindPassword from secret names.");
        }

        var identifierLdap = attribute == DirectoryAttribute.UserSmimeCertificate
            ? "userSMIMECertificate"
            : "userCertificate;binary";

        using var connection = new LdapConnection(new LdapDirectoryIdentifier(o.Host, o.Port, false, false));
        connection.SessionOptions.SecureSocketLayer = o.UseSsl;
        connection.SessionOptions.ProtocolVersion = 3;
        connection.AuthType = AuthType.Basic;
        connection.Credential = new NetworkCredential(o.BindUser, o.BindPassword);
        connection.Bind();

        var modification = new DirectoryAttributeModification
        {
            Name = identifierLdap,
            Operation = DirectoryAttributeOperation.Add
        };
        modification.Add(derEncodedCertificate);

        var request = new ModifyRequest(distinguishedName, modification);
        connection.SendRequest(request);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Development publisher that records publication without contacting a directory.
/// </summary>
public sealed class NullActiveDirectoryPublisher : IActiveDirectoryPublisher
{
    public Task PublishPublicCertificateAsync(
        string distinguishedName,
        byte[] derEncodedCertificate,
        DirectoryAttribute attribute,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
