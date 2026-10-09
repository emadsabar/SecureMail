using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Enrollments;

public sealed record SubmitCsrCommand(Guid EnrollmentId, Guid DeviceId, string CsrPem) : IRequest;

public sealed class SubmitCsrCommandValidator : AbstractValidator<SubmitCsrCommand>
{
    public SubmitCsrCommandValidator()
    {
        RuleFor(x => x.EnrollmentId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.CsrPem).NotEmpty().Must(pem => pem.Contains("BEGIN CERTIFICATE REQUEST", StringComparison.OrdinalIgnoreCase))
            .WithMessage("A PEM-encoded PKCS#10 certificate request is required.");
        RuleFor(x => x.CsrPem).Must(pem => !pem.Contains("PRIVATE KEY", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Private keys must never be submitted to the control plane.");
    }
}

public sealed class SubmitCsrCommandHandler(
    IApplicationDbContext db,
    ICertificateAuthority ca,
    IAuditService audit) : IRequestHandler<SubmitCsrCommand>
{
    public async Task Handle(SubmitCsrCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments.FirstOrDefaultAsync(e => e.Id == request.EnrollmentId, cancellationToken)
                         ?? throw new DomainException("Enrollment was not found.");

        var user = await db.DirectoryUsers.FirstAsync(u => u.Id == enrollment.DirectoryUserId, cancellationToken);
        enrollment.AttachCsr(request.CsrPem);

        var caRequestId = await ca.SubmitEnrollmentAsync(
            new CertificateAuthorityRequest(
                request.CsrPem,
                enrollment.Subject,
                user.Mail,
                new Dictionary<string, string>()),
            cancellationToken);

        enrollment.MarkSubmitted(caRequestId);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Agent,
            request.DeviceId.ToString(),
            "CsrSubmitted",
            nameof(CertificateEnrollment),
            enrollment.Id,
            "CSR submitted to certificate authority adapter.",
            cancellationToken);
    }
}
