using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Devices;

public sealed record RegisterDeviceCommand(
    Guid DirectoryUserId,
    string Hostname,
    string OsVersion,
    string AgentVersion,
    string? EntraDeviceId,
    string EnrollmentSecret) : IRequest<Guid>;

public sealed class RegisterDeviceCommandHandler(
    IApplicationDbContext db,
    IAuditService audit) : IRequestHandler<RegisterDeviceCommand, Guid>
{
    public async Task<Guid> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var user = await db.DirectoryUsers.FirstOrDefaultAsync(u => u.Id == request.DirectoryUserId, cancellationToken)
                   ?? throw new DomainException("Directory user was not found.");

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.EnrollmentSecret)));
        var device = await db.ManagedDevices.FirstOrDefaultAsync(
            d => d.DirectoryUserId == user.Id && d.Hostname == request.Hostname, cancellationToken);

        if (device is null)
        {
            device = ManagedDevice.Register(user.Id, request.Hostname, request.OsVersion, request.AgentVersion, hash, request.EntraDeviceId);
            db.ManagedDevices.Add(device);
        }
        else
        {
            device.Heartbeat(request.AgentVersion, request.OsVersion);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Agent,
            request.Hostname,
            "DeviceRegistered",
            nameof(ManagedDevice),
            device.Id,
            $"os={request.OsVersion}",
            cancellationToken);
        return device.Id;
    }
}

public sealed record GetPendingEnrollmentQuery(Guid DeviceId) : IRequest<PendingEnrollmentDto?>;

public sealed record PendingEnrollmentDto(Guid EnrollmentId, Guid DirectoryUserId, string Subject, string Mail);

public sealed class GetPendingEnrollmentQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPendingEnrollmentQuery, PendingEnrollmentDto?>
{
    public async Task<PendingEnrollmentDto?> Handle(GetPendingEnrollmentQuery request, CancellationToken cancellationToken)
    {
        var device = await db.ManagedDevices.FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);
        if (device is null)
        {
            return null;
        }

        var enrollment = await db.Enrollments
            .Where(e => e.DirectoryUserId == device.DirectoryUserId && e.Status == EnrollmentStatus.AwaitingCsr)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment is null)
        {
            return null;
        }

        var user = await db.DirectoryUsers.FirstAsync(u => u.Id == enrollment.DirectoryUserId, cancellationToken);
        return new PendingEnrollmentDto(enrollment.Id, user.Id, enrollment.Subject, user.Mail);
    }
}

public sealed record GetIssuedPublicCertificateQuery(Guid EnrollmentId) : IRequest<IssuedPublicCertificateDto?>;

public sealed record IssuedPublicCertificateDto(Guid CertificateId, string Thumbprint, string PublicCertificatePem);

public sealed class GetIssuedPublicCertificateQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetIssuedPublicCertificateQuery, IssuedPublicCertificateDto?>
{
    public async Task<IssuedPublicCertificateDto?> Handle(GetIssuedPublicCertificateQuery request, CancellationToken cancellationToken)
    {
        var cert = await db.Certificates.FirstOrDefaultAsync(c => c.EnrollmentId == request.EnrollmentId, cancellationToken);
        return cert is null
            ? null
            : new IssuedPublicCertificateDto(cert.Id, cert.Thumbprint, cert.PublicCertificatePem);
    }
}
