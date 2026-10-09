using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Dashboard;

public sealed record DashboardSummary(
    int EligibleUsers,
    int ActiveCertificates,
    int ExpiringSoon,
    int Failed,
    int PendingIssuance,
    int DevicesSeen,
    IReadOnlyList<CertificateListItem> Certificates);

public sealed record CertificateListItem(
    Guid Id,
    string UserPrincipalName,
    string Thumbprint,
    LifecycleStatus Status,
    DateTimeOffset NotAfter,
    string? Hostname);

public sealed record GetDashboardQuery(int ExpiryWarningDays = 30) : IRequest<DashboardSummary>;

public sealed class GetDashboardQueryHandler(IApplicationDbContext db) : IRequestHandler<GetDashboardQuery, DashboardSummary>
{
    public async Task<DashboardSummary> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var warning = now.AddDays(request.ExpiryWarningDays);

        var users = await db.DirectoryUsers.CountAsync(u => u.IsEligible, cancellationToken);
        var active = await db.Certificates.CountAsync(c => c.Status == LifecycleStatus.Active, cancellationToken);
        var expiring = await db.Certificates.CountAsync(
            c => c.Status == LifecycleStatus.Active && c.NotAfter <= warning, cancellationToken);
        var failed = await db.Certificates.CountAsync(c => c.Status == LifecycleStatus.Failed, cancellationToken)
                     + await db.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Failed, cancellationToken);
        var pending = await db.Enrollments.CountAsync(
            e => e.Status == EnrollmentStatus.SubmittedToCa || e.Status == EnrollmentStatus.CsrReceived, cancellationToken);
        var devices = await db.ManagedDevices.CountAsync(cancellationToken);

        var items = await (
                from c in db.Certificates
                join u in db.DirectoryUsers on c.DirectoryUserId equals u.Id
                join d in db.ManagedDevices on c.InstalledOnDeviceId equals d.Id into dj
                from d in dj.DefaultIfEmpty()
                orderby c.UpdatedAt descending
                select new CertificateListItem(
                    c.Id,
                    u.UserPrincipalName,
                    c.Thumbprint,
                    c.Status,
                    c.NotAfter,
                    d != null ? d.Hostname : null))
            .Take(100)
            .ToListAsync(cancellationToken);

        return new DashboardSummary(users, active, expiring, failed, pending, devices, items);
    }
}
