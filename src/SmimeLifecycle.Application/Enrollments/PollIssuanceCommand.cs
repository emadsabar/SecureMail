using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Enrollments;

public sealed record PollIssuanceCommand : IRequest<int>;

public sealed class PollIssuanceCommandHandler(
    IApplicationDbContext db,
    ICertificateAuthority ca,
    IPublicCertificateParser parser,
    IAuditService audit) : IRequestHandler<PollIssuanceCommand, int>
{
    public async Task<int> Handle(PollIssuanceCommand request, CancellationToken cancellationToken)
    {
        var pending = await db.Enrollments
            .Where(e => e.Status == EnrollmentStatus.SubmittedToCa && e.CaRequestId != null)
            .ToListAsync(cancellationToken);

        var issued = 0;
        foreach (var enrollment in pending)
        {
            var status = await ca.GetStatusAsync(enrollment.CaRequestId!, cancellationToken);
            switch (status.State)
            {
                case CertificateAuthorityRequestState.Issued when status.PublicCertificatePem is not null:
                    var parsed = parser.ParsePem(status.PublicCertificatePem);
                    var certificate = IssuedCertificate.FromPublicMaterial(
                        enrollment.Id,
                        enrollment.DirectoryUserId,
                        parsed.Thumbprint,
                        parsed.SerialNumber,
                        parsed.Subject,
                        parsed.Issuer,
                        parsed.NotBefore,
                        parsed.NotAfter,
                        parsed.Pem);
                    db.Certificates.Add(certificate);
                    db.Publications.Add(DirectoryPublication.Create(certificate.Id, DirectoryAttribute.UserCertificate));
                    db.Publications.Add(DirectoryPublication.Create(certificate.Id, DirectoryAttribute.UserSmimeCertificate));
                    enrollment.MarkIssued();
                    issued++;
                    await audit.WriteAsync(
                        AuditActorType.System,
                        "worker",
                        "CertificateIssued",
                        nameof(IssuedCertificate),
                        certificate.Id,
                        $"thumbprint={parsed.Thumbprint}",
                        cancellationToken);
                    break;
                case CertificateAuthorityRequestState.Rejected:
                    enrollment.MarkFailed(status.Detail ?? "CA rejected the request.");
                    break;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return issued;
    }
}
