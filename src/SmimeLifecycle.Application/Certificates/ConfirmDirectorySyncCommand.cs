using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Certificates;

public sealed record ConfirmDirectorySyncCommand(Guid CertificateId) : IRequest;

public sealed class ConfirmDirectorySyncCommandHandler(
    IApplicationDbContext db,
    IAuditService audit) : IRequestHandler<ConfirmDirectorySyncCommand>
{
    public async Task Handle(ConfirmDirectorySyncCommand request, CancellationToken cancellationToken)
    {
        var certificate = await db.Certificates.FirstOrDefaultAsync(c => c.Id == request.CertificateId, cancellationToken)
                          ?? throw new DomainException("Certificate was not found.");

        certificate.MarkDirectorySynced();
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Administrator,
            "admin",
            "DirectorySyncConfirmed",
            nameof(IssuedCertificate),
            certificate.Id,
            "On-premises Active Directory publication of userCertificate/userSMIMECertificate verified.",
            cancellationToken);
    }
}
