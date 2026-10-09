using SmimeLifecycle.Domain.Common;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Domain.Entities;

public sealed class OutlookReadiness : Entity
{
    public Guid DirectoryUserId { get; private set; }
    public Guid DeviceId { get; private set; }
    public OutlookClientProfile ClientProfile { get; private set; }
    public OutlookReadinessStatus Status { get; private set; } = OutlookReadinessStatus.Unknown;
    public string? SigningThumbprint { get; private set; }
    public string? Detail { get; private set; }
    public DateTimeOffset LastCheckedAt { get; private set; } = DateTimeOffset.UtcNow;

    private OutlookReadiness()
    {
    }

    public static OutlookReadiness Create(Guid directoryUserId, Guid deviceId, OutlookClientProfile profile)
    {
        return new OutlookReadiness
        {
            DirectoryUserId = directoryUserId,
            DeviceId = deviceId,
            ClientProfile = profile,
            Status = profile == OutlookClientProfile.ClassicOutlookWindows
                ? OutlookReadinessStatus.Unknown
                : OutlookReadinessStatus.UnsupportedClient,
            Detail = profile == OutlookClientProfile.ClassicOutlookWindows
                ? null
                : "This client is out of Phase 1 support. Validate separately before claiming compatibility."
        };
    }

    public void RecordCheck(OutlookReadinessStatus status, string? signingThumbprint, string? detail)
    {
        if (ClientProfile != OutlookClientProfile.ClassicOutlookWindows)
        {
            Status = OutlookReadinessStatus.UnsupportedClient;
            Detail = "Phase 1 supports classic Outlook for Windows only.";
            LastCheckedAt = DateTimeOffset.UtcNow;
            Touch();
            return;
        }

        Status = status;
        SigningThumbprint = signingThumbprint;
        Detail = detail;
        LastCheckedAt = DateTimeOffset.UtcNow;
        Touch();
    }
}
