using Microsoft.Extensions.Options;
using SmimeLifecycle.Application.Abstractions;

namespace SmimeLifecycle.Infrastructure.Identity;

public sealed class IdentityDirectoryOptions
{
    public const string SectionName = "IdentityDirectory";
    /// <summary>Configured | ActiveDirectory</summary>
    public string Provider { get; set; } = "Configured";
    public List<ConfiguredIdentityUser> Users { get; set; } = [];
    /// <summary>AD group DN whose members are S/MIME eligible.</summary>
    public string? EligibleGroupDn { get; set; }
}

public sealed class ConfiguredIdentityUser
{
    public string DirectoryObjectGuid { get; set; } = Guid.NewGuid().ToString();
    public string UserPrincipalName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DistinguishedName { get; set; }
}

/// <summary>
/// Lab/dev identity source. Production on-premises deployments use <see cref="ActiveDirectoryIdentityDirectory"/>.
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
        new(u.DirectoryObjectGuid, u.UserPrincipalName, u.Mail, u.DisplayName, u.DistinguishedName, []);
}
