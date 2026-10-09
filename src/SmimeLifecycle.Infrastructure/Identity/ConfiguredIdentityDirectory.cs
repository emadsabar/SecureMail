using Microsoft.Extensions.Options;
using SmimeLifecycle.Application.Abstractions;

namespace SmimeLifecycle.Infrastructure.Identity;

public sealed class IdentityDirectoryOptions
{
    public const string SectionName = "IdentityDirectory";
    public List<ConfiguredIdentityUser> Users { get; set; } = [];
    public string? EligibleGroupId { get; set; }
}

public sealed class ConfiguredIdentityUser
{
    public string EntraObjectId { get; set; } = Guid.NewGuid().ToString();
    public string UserPrincipalName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? OnPremisesDistinguishedName { get; set; }
}

/// <summary>
/// Development/test identity source. Production should replace this with a Microsoft Graph adapter
/// that reads Entra ID users and group membership using application permissions.
/// </summary>
public sealed class ConfiguredIdentityDirectory(IOptions<IdentityDirectoryOptions> options) : IIdentityDirectory
{
    public Task<IdentityUserSnapshot?> GetByUserPrincipalNameAsync(string upn, CancellationToken cancellationToken)
    {
        var match = options.Value.Users.FirstOrDefault(u =>
            string.Equals(u.UserPrincipalName, upn, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match is null ? null : Map(match));
    }

    public Task<IReadOnlyList<IdentityUserSnapshot>> ListEligibleCandidatesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<IdentityUserSnapshot> list = options.Value.Users.Select(Map).ToList();
        return Task.FromResult(list);
    }

    private static IdentityUserSnapshot Map(ConfiguredIdentityUser u) =>
        new(u.EntraObjectId, u.UserPrincipalName, u.Mail, u.DisplayName, u.OnPremisesDistinguishedName, []);
}
