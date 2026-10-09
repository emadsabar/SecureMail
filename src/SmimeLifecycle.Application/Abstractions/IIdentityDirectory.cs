namespace SmimeLifecycle.Application.Abstractions;

public sealed record IdentityUserSnapshot(
    string EntraObjectId,
    string UserPrincipalName,
    string Mail,
    string DisplayName,
    string? OnPremisesDistinguishedName,
    IReadOnlyCollection<string> GroupIds);

public interface IIdentityDirectory
{
    Task<IdentityUserSnapshot?> GetByUserPrincipalNameAsync(string upn, CancellationToken cancellationToken);

    Task<IReadOnlyList<IdentityUserSnapshot>> ListEligibleCandidatesAsync(CancellationToken cancellationToken);
}
