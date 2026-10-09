using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Outlook;

public sealed record RecordOutlookCheckCommand(
    Guid DirectoryUserId,
    Guid DeviceId,
    OutlookClientProfile Profile,
    OutlookReadinessStatus Status,
    string? SigningThumbprint,
    string? Detail) : IRequest;

public sealed class RecordOutlookCheckCommandHandler(
    IApplicationDbContext db,
    IAuditService audit) : IRequestHandler<RecordOutlookCheckCommand>
{
    public async Task Handle(RecordOutlookCheckCommand request, CancellationToken cancellationToken)
    {
        var row = await db.OutlookReadiness.FirstOrDefaultAsync(
            o => o.DirectoryUserId == request.DirectoryUserId && o.DeviceId == request.DeviceId && o.ClientProfile == request.Profile,
            cancellationToken);

        if (row is null)
        {
            row = OutlookReadiness.Create(request.DirectoryUserId, request.DeviceId, request.Profile);
            db.OutlookReadiness.Add(row);
        }

        row.RecordCheck(request.Status, request.SigningThumbprint, request.Detail);

        if (request.Profile == OutlookClientProfile.ClassicOutlookWindows &&
            request.Status is OutlookReadinessStatus.CertificatePresent or OutlookReadinessStatus.SigningReady &&
            request.SigningThumbprint is not null)
        {
            var certificate = await db.Certificates.FirstOrDefaultAsync(
                c => c.Thumbprint == request.SigningThumbprint.Replace(" ", string.Empty).ToUpperInvariant(),
                cancellationToken);
            if (certificate is { Status: LifecycleStatus.DirectorySyncPending })
            {
                certificate.MarkDirectorySynced();
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Agent,
            request.DeviceId.ToString(),
            "OutlookReadinessChecked",
            nameof(OutlookReadiness),
            row.Id,
            request.Detail ?? request.Status.ToString(),
            cancellationToken);
    }
}
