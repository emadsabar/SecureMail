using FluentAssertions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Domain.Tests;

public class IssuedCertificateTests
{
    [Fact]
    public void Rejects_private_key_material()
    {
        var act = () => IssuedCertificate.FromPublicMaterial(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "AA",
            "1",
            "CN=test",
            "CN=ca",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1),
            "-----BEGIN PRIVATE KEY-----\nabc\n-----END PRIVATE KEY-----");

        act.Should().Throw<SecretStorageException>();
    }

    [Fact]
    public void Audit_event_redacts_secret_like_payloads()
    {
        var evt = AuditEvent.Record(
            Domain.Enums.AuditActorType.System,
            "worker",
            "test",
            "IssuedCertificate",
            Guid.NewGuid(),
            "access_token=super-secret");

        evt.Details.Should().Contain("redacted");
    }
}
