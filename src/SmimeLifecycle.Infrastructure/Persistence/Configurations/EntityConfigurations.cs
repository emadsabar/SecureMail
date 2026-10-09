using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmimeLifecycle.Domain.Entities;

namespace SmimeLifecycle.Infrastructure.Persistence.Configurations;

internal sealed class DirectoryUserConfiguration : IEntityTypeConfiguration<DirectoryUser>
{
    public void Configure(EntityTypeBuilder<DirectoryUser> builder)
    {
        builder.ToTable("directory_users");
        builder.HasIndex(x => x.EntraObjectId).IsUnique();
        builder.HasIndex(x => x.UserPrincipalName).IsUnique();
        builder.Property(x => x.UserPrincipalName).HasMaxLength(320);
        builder.Property(x => x.Mail).HasMaxLength(320);
    }
}

internal sealed class ManagedDeviceConfiguration : IEntityTypeConfiguration<ManagedDevice>
{
    public void Configure(EntityTypeBuilder<ManagedDevice> builder)
    {
        builder.ToTable("managed_devices");
        builder.Property(x => x.Hostname).HasMaxLength(255);
        builder.Property(x => x.EnrollmentSecretHash).HasMaxLength(128);
        builder.HasIndex(x => new { x.DirectoryUserId, x.Hostname }).IsUnique();
    }
}

internal sealed class CertificateEnrollmentConfiguration : IEntityTypeConfiguration<CertificateEnrollment>
{
    public void Configure(EntityTypeBuilder<CertificateEnrollment> builder)
    {
        builder.ToTable("certificate_enrollments");
        builder.Property(x => x.CsrPem).HasColumnType("text");
        builder.Property(x => x.Subject).HasMaxLength(512);
        builder.Property(x => x.CaRequestId).HasMaxLength(256);
    }
}

internal sealed class IssuedCertificateConfiguration : IEntityTypeConfiguration<IssuedCertificate>
{
    public void Configure(EntityTypeBuilder<IssuedCertificate> builder)
    {
        builder.ToTable("issued_certificates");
        builder.HasIndex(x => x.Thumbprint).IsUnique();
        builder.Property(x => x.Thumbprint).HasMaxLength(64);
        builder.Property(x => x.PublicCertificatePem).HasColumnType("text");
    }
}

internal sealed class DirectoryPublicationConfiguration : IEntityTypeConfiguration<DirectoryPublication>
{
    public void Configure(EntityTypeBuilder<DirectoryPublication> builder)
    {
        builder.ToTable("directory_publications");
        builder.HasIndex(x => new { x.CertificateId, x.Attribute }).IsUnique();
    }
}

internal sealed class OutlookReadinessConfiguration : IEntityTypeConfiguration<OutlookReadiness>
{
    public void Configure(EntityTypeBuilder<OutlookReadiness> builder)
    {
        builder.ToTable("outlook_readiness");
        builder.HasIndex(x => new { x.DirectoryUserId, x.DeviceId, x.ClientProfile }).IsUnique();
    }
}

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.Property(x => x.Details).HasColumnType("text");
        builder.HasIndex(x => x.CreatedAt);
    }
}
