using System.Security.Cryptography.X509Certificates;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Certificates;

public sealed record PublishCertificatesCommand : IRequest<int>;

public sealed class PublishCertificatesCommandHandler(
    IApplicationDbContext db,
    IActiveDirectoryPublisher publisher,
    IAuditService audit) : IRequestHandler<PublishCertificatesCommand, int>
{
    public async Task<int> Handle(PublishCertificatesCommand request, CancellationToken cancellationToken)
    {
        var candidates = await db.Certificates
            .Where(c => c.Status == LifecycleStatus.Installed)
            .ToListAsync(cancellationToken);

        var published = 0;
        foreach (var certificate in candidates)
        {
            var user = await db.DirectoryUsers.FirstAsync(u => u.Id == certificate.DirectoryUserId, cancellationToken);
            if (string.IsNullOrWhiteSpace(user.OnPremisesDistinguishedName))
            {
                continue;
            }

            var publications = await db.Publications
                .Where(p => p.CertificateId == certificate.Id)
                .ToListAsync(cancellationToken);

            var der = PemToDer(certificate.PublicCertificatePem);
            var allOk = true;
            foreach (var publication in publications)
            {
                publication.MarkInProgress();
                try
                {
                    await publisher.PublishPublicCertificateAsync(
                        user.OnPremisesDistinguishedName,
                        der,
                        publication.Attribute,
                        cancellationToken);
                    publication.MarkPublished();
                }
                catch (Exception ex)
                {
                    publication.MarkFailed(ex.Message);
                    allOk = false;
                }
            }

            if (allOk)
            {
                certificate.MarkPublished();
                published++;
                await audit.WriteAsync(
                    AuditActorType.System,
                    "worker",
                    "CertificatePublished",
                    nameof(IssuedCertificate),
                    certificate.Id,
                    "Published public certificate to Active Directory attributes.",
                    cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return published;
    }

    private static byte[] PemToDer(string pem)
    {
        using var cert = X509Certificate2.CreateFromPem(pem);
        return cert.Export(X509ContentType.Cert);
    }
}
