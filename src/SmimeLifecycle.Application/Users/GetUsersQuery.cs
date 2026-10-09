using MediatR;
using Microsoft.EntityFrameworkCore;
using SmimeLifecycle.Application.Abstractions;

namespace SmimeLifecycle.Application.Users;

public sealed record UserListItem(
    Guid Id,
    string UserPrincipalName,
    string Mail,
    string DisplayName,
    bool IsEligible,
    string? DistinguishedName);

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserListItem>>;

public sealed class GetUsersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItem>>
{
    public async Task<IReadOnlyList<UserListItem>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await db.DirectoryUsers
            .OrderBy(u => u.UserPrincipalName)
            .Select(u => new UserListItem(u.Id, u.UserPrincipalName, u.Mail, u.DisplayName, u.IsEligible, u.DistinguishedName))
            .ToListAsync(cancellationToken);
    }
}
