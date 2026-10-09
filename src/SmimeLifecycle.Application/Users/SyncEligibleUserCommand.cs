using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Domain.Entities;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.Application.Users;

public sealed record SyncEligibleUserCommand(string UserPrincipalName) : IRequest<Guid>;

public sealed class SyncEligibleUserCommandHandler(
    IApplicationDbContext db,
    IIdentityDirectory identity,
    IAuditService audit) : IRequestHandler<SyncEligibleUserCommand, Guid>
{
    public async Task<Guid> Handle(SyncEligibleUserCommand request, CancellationToken cancellationToken)
    {
        var snapshot = await identity.GetByUserPrincipalNameAsync(request.UserPrincipalName, cancellationToken)
                       ?? throw new InvalidOperationException($"User '{request.UserPrincipalName}' was not found in the identity directory.");

        var user = await db.DirectoryUsers.FirstOrDefaultAsync(u => u.EntraObjectId == snapshot.EntraObjectId, cancellationToken);
        if (user is null)
        {
            user = DirectoryUser.Create(
                snapshot.EntraObjectId,
                snapshot.UserPrincipalName,
                snapshot.Mail,
                snapshot.DisplayName,
                snapshot.OnPremisesDistinguishedName,
                isEligible: true,
                eligibilityReason: "Matched Phase 1 eligibility policy.");
            db.DirectoryUsers.Add(user);
        }
        else
        {
            user.UpdateEligibility(true, "Matched Phase 1 eligibility policy.");
            user.UpdateDirectoryBinding(snapshot.OnPremisesDistinguishedName);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            AuditActorType.Administrator,
            "admin",
            "UserEligible",
            nameof(DirectoryUser),
            user.Id,
            $"UPN={user.UserPrincipalName}",
            cancellationToken);
        return user.Id;
    }
}
