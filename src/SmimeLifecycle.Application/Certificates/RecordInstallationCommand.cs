using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Certificates;

public sealed record RecordInstallationCommand(Guid CertificateId, Guid DeviceId, string Thumbprint) : IRequest;

public sealed class RecordInstallationCommandHandler(
    IApplicationDbContext db,
    IAuditService audit) : IRequestHandler<RecordInstallationCommand>
{
    public async Task Handle(RecordInstallationCommand request, CancellationToken cancellationToken)
    {
        var certificate = await db.Certificates.FirstOrDefaultAsync(c => c.Id == request.CertificateId, cancellationToken)
                          ?? throw new DomainException("Certificate was not found.");

        if (!string.Equals(certificate.Thumbprint, request.Thumbprint.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Installed thumbprint does not match the issued public certificate.");
        }

        certificate.MarkInstalled(request.DeviceId);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Agent,
            request.DeviceId.ToString(),
            "CertificateInstalled",
            nameof(IssuedCertificate),
            certificate.Id,
            "Installed into CurrentUser\\My.",
            cancellationToken);
    }
}
