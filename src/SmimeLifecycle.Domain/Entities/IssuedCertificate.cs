using SmimeLifecycle.Domain.Common;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;
using SmimeLifecycle.Domain.Lifecycle;

namespace SmimeLifecycle.Domain.Entities;

public sealed class IssuedCertificate : Entity
{
    public Guid EnrollmentId { get; private set; }
    public Guid DirectoryUserId { get; private set; }
    public Guid? InstalledOnDeviceId { get; private set; }
    public LifecycleStatus Status { get; private set; } = LifecycleStatus.Issued;
    public string Thumbprint { get; private set; } = string.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Issuer { get; private set; } = string.Empty;
    public DateTimeOffset NotBefore { get; private set; }
    public DateTimeOffset NotAfter { get; private set; }
    public string PublicCertificatePem { get; private set; } = string.Empty;
    public DateTimeOffset? InstalledAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? DirectorySyncObservedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }

    private IssuedCertificate()
    {
    }

    public static IssuedCertificate FromPublicMaterial(
        Guid enrollmentId,
        Guid directoryUserId,
        string thumbprint,
        string serialNumber,
        string subject,
        string issuer,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        string publicCertificatePem)
    {
        GuardPublicOnly(publicCertificatePem);
        return new IssuedCertificate
        {
            EnrollmentId = enrollmentId,
            DirectoryUserId = directoryUserId,
            Thumbprint = thumbprint.Replace(" ", string.Empty).ToUpperInvariant(),
            SerialNumber = serialNumber,
            Subject = subject,
            Issuer = issuer,
            NotBefore = notBefore,
            NotAfter = notAfter,
            PublicCertificatePem = publicCertificatePem,
            Status = LifecycleStatus.Issued
        };
    }

    public void MarkInstalled(Guid deviceId)
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.Installed);
        InstalledOnDeviceId = deviceId;
        InstalledAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void MarkPublished()
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.PublishedToDirectory);
        PublishedAt = DateTimeOffset.UtcNow;
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.DirectorySyncPending);
        Touch();
    }

    public void MarkDirectorySynced()
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.Active);
        DirectorySyncObservedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void MarkRevoked(string reason)
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.Revoked);
        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = reason;
        Touch();
    }

    public void MarkFailed()
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.Failed);
        Touch();
    }

    public void BeginRenewal()
    {
        Status = CertificateLifecycleMachine.Transition(Status, LifecycleStatus.Renewing);
        Touch();
    }

    public bool IsExpiringWithin(TimeSpan window, DateTimeOffset utcNow) =>
        Status == LifecycleStatus.Active && NotAfter <= utcNow.Add(window);

    private static void GuardPublicOnly(string pem)
    {
        if (pem.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase) ||
            pem.Contains("BEGIN PKCS12", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecretStorageException();
        }
    }
}
