using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Domain.Entities;

namespace SmimeLifecycle.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<DirectoryUser> DirectoryUsers { get; }
    DbSet<ManagedDevice> ManagedDevices { get; }
    DbSet<CertificateEnrollment> Enrollments { get; }
    DbSet<IssuedCertificate> Certificates { get; }
    DbSet<DirectoryPublication> Publications { get; }
    DbSet<OutlookReadiness> OutlookReadiness { get; }
    DbSet<AuditEvent> AuditEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
