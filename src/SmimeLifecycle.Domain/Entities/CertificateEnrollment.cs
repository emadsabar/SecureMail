using SmimeLifecycle.Domain.Common;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Domain.Entities;

public sealed class CertificateEnrollment : Entity
{
    public Guid DirectoryUserId { get; private set; }
    public Guid? DeviceId { get; private set; }
    public EnrollmentStatus Status { get; private set; } = EnrollmentStatus.AwaitingCsr;
    public string? CsrPem { get; private set; }
    public string? CaRequestId { get; private set; }
    public string? FailureReason { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public bool IsRenewal { get; private set; }

    private CertificateEnrollment()
    {
    }

    public static CertificateEnrollment Start(Guid directoryUserId, Guid? deviceId, string subject, bool isRenewal)
    {
        return new CertificateEnrollment
        {
            DirectoryUserId = directoryUserId,
            DeviceId = deviceId,
            Subject = subject,
            IsRenewal = isRenewal,
            Status = EnrollmentStatus.AwaitingCsr
        };
    }

    public void AttachCsr(string csrPem)
    {
        EnsureNoPrivateMaterial(csrPem);
        if (Status is not EnrollmentStatus.AwaitingCsr and not EnrollmentStatus.Failed)
        {
            throw new DomainException($"Cannot attach a CSR while enrollment is '{Status}'.");
        }

        CsrPem = csrPem;
        Status = EnrollmentStatus.CsrReceived;
        FailureReason = null;
        Touch();
    }

    public void MarkSubmitted(string caRequestId)
    {
        if (Status != EnrollmentStatus.CsrReceived)
        {
            throw new DomainException("A CSR must be received before submitting to the CA.");
        }

        CaRequestId = caRequestId;
        Status = EnrollmentStatus.SubmittedToCa;
        Touch();
    }

    public void MarkIssued()
    {
        if (Status != EnrollmentStatus.SubmittedToCa)
        {
            throw new DomainException("Issuance can only complete a submitted CA request.");
        }

        Status = EnrollmentStatus.Issued;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        Status = EnrollmentStatus.Failed;
        FailureReason = reason;
        Touch();
    }

    public void Cancel()
    {
        Status = EnrollmentStatus.Cancelled;
        Touch();
    }

    private static void EnsureNoPrivateMaterial(string pem)
    {
        if (pem.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase) ||
            pem.Contains("PFX", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecretStorageException();
        }
    }
}
