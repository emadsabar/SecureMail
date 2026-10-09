namespace SmimeLifecycle.Application.Abstractions;

/// <summary>
/// On-premises directory identity source (Active Directory). Not a cloud identity provider.
/// </summary>
public sealed record IdentityUserSnapshot(
    string DirectoryObjectGuid,
    string UserPrincipalName,
    string Mail,
    string DisplayName,
    string? DistinguishedName,
    IReadOnlyCollection<string> GroupDns);

public interface IIdentityDirectory
{
    Task<IdentityUserSnapshot?> GetByUserPrincipalNameAsync(string upn, CancellationToken cancellationToken);

    Task<IReadOnlyList<IdentityUserSnapshot>> ListEligibleCandidatesAsync(CancellationToken cancellationToken);
}
