using SmimeLifecycle.Domain.Common;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Domain.Entities;

public sealed class DirectoryPublication : Entity
{
    public Guid CertificateId { get; private set; }
    public DirectoryAttribute Attribute { get; private set; }
    public DirectoryPublicationStatus Status { get; private set; } = DirectoryPublicationStatus.NotStarted;
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }

    private DirectoryPublication()
    {
    }

    public static DirectoryPublication Create(Guid certificateId, DirectoryAttribute attribute)
    {
        return new DirectoryPublication
        {
            CertificateId = certificateId,
            Attribute = attribute
        };
    }

    public void MarkInProgress()
    {
        Status = DirectoryPublicationStatus.InProgress;
        AttemptCount++;
        Touch();
    }

    public void MarkPublished()
    {
        Status = DirectoryPublicationStatus.Published;
        LastError = null;
        Touch();
    }

    public void MarkFailed(string error)
    {
        Status = DirectoryPublicationStatus.Failed;
        LastError = error;
        Touch();
    }
}
