using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Abstractions;

public interface IActiveDirectoryPublisher
{
    Task PublishPublicCertificateAsync(
        string distinguishedName,
        byte[] derEncodedCertificate,
        DirectoryAttribute attribute,
        CancellationToken cancellationToken);
}
