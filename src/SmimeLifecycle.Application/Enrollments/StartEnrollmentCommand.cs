using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;
using SmimeLifecycle.Domain.Exceptions;

namespace SmimeLifecycle.Application.Enrollments;

public sealed record StartEnrollmentCommand(Guid DirectoryUserId, Guid? DeviceId, bool IsRenewal) : IRequest<Guid>;

public sealed class StartEnrollmentCommandValidator : AbstractValidator<StartEnrollmentCommand>
{
    public StartEnrollmentCommandValidator()
    {
        RuleFor(x => x.DirectoryUserId).NotEmpty();
    }
}

public sealed class StartEnrollmentCommandHandler(
    IApplicationDbContext db,
    IAuditService audit) : IRequestHandler<StartEnrollmentCommand, Guid>
{
    public async Task<Guid> Handle(StartEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var user = await db.DirectoryUsers.FirstOrDefaultAsync(u => u.Id == request.DirectoryUserId, cancellationToken)
                   ?? throw new DomainException("Directory user was not found.");

        if (!user.IsEligible)
        {
            throw new DomainException("User is not eligible for S/MIME enrollment.");
        }

        var subject = $"CN={user.Mail}";
        var enrollment = CertificateEnrollment.Start(user.Id, request.DeviceId, subject, request.IsRenewal);
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.System,
            "api",
            "EnrollmentStarted",
            nameof(CertificateEnrollment),
            enrollment.Id,
            $"user={user.UserPrincipalName};renewal={request.IsRenewal}",
            cancellationToken);
        return enrollment.Id;
    }
}
