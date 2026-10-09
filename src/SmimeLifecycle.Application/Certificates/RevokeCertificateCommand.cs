using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Certificates;

public sealed record RevokeCertificateCommand(Guid CertificateId, string Reason) : IRequest;

public sealed class RevokeCertificateCommandHandler(
    IApplicationDbContext db,
    ICertificateAuthority ca,
    IAuditService audit) : IRequestHandler<RevokeCertificateCommand>
{
    public async Task Handle(RevokeCertificateCommand request, CancellationToken cancellationToken)
    {
        var certificate = await db.Certificates.FirstOrDefaultAsync(c => c.Id == request.CertificateId, cancellationToken)
                          ?? throw new DomainException("Certificate was not found.");

        var enrollment = await db.Enrollments.FirstAsync(e => e.Id == certificate.EnrollmentId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(enrollment.CaRequestId))
        {
            await ca.RevokeAsync(enrollment.CaRequestId, request.Reason, cancellationToken);
        }

        certificate.MarkRevoked(request.Reason);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Administrator,
            "admin",
            "CertificateRevoked",
            nameof(IssuedCertificate),
            certificate.Id,
            request.Reason,
            cancellationToken);
    }
}
