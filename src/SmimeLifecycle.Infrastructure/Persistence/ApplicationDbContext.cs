using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;

namespace SmimeLifecycle.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<DirectoryUser> DirectoryUsers => Set<DirectoryUser>();
    public DbSet<ManagedDevice> ManagedDevices => Set<ManagedDevice>();
    public DbSet<CertificateEnrollment> Enrollments => Set<CertificateEnrollment>();
    public DbSet<IssuedCertificate> Certificates => Set<IssuedCertificate>();
    public DbSet<DirectoryPublication> Publications => Set<DirectoryPublication>();
    public DbSet<OutlookReadiness> OutlookReadiness => Set<OutlookReadiness>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
