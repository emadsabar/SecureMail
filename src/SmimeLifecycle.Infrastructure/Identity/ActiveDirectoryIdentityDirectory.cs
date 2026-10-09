using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Infrastructure.Directory;

namespace SmimeLifecycle.Infrastructure.Identity;

/// <summary>
/// Resolves eligible users from on-premises Active Directory over LDAPS.
/// </summary>
public sealed class ActiveDirectoryIdentityDirectory(
    IOptions<ActiveDirectoryOptions> directoryOptions,
    IOptions<IdentityDirectoryOptions> identityOptions,
    ILogger<ActiveDirectoryIdentityDirectory> logger) : IIdentityDirectory
{
    public Task<IdentityUserSnapshot?> GetByUserPrincipalNameAsync(string upn, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var connection = Bind();
        var filter = $"(&(objectCategory=person)(objectClass=user)(userPrincipalName={Escape(upn)}))";
        var request = new SearchRequest(
            GetSearchBase(),
            filter,
            SearchScope.Subtree,
            "objectGUID", "userPrincipalName", "mail", "displayName", "distinguishedName", "memberOf");

        var response = (SearchResponse)connection.SendRequest(request);
        var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();
        return Task.FromResult(entry is null ? null : Map(entry));
    }

    public Task<IReadOnlyList<IdentityUserSnapshot>> ListEligibleCandidatesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var groupDn = identityOptions.Value.EligibleGroupDn;
        if (string.IsNullOrWhiteSpace(groupDn))
        {
            logger.LogWarning("IdentityDirectory:EligibleGroupDn is not configured; returning no eligible users.");
            return Task.FromResult<IReadOnlyList<IdentityUserSnapshot>>([]);
        }

        using var connection = Bind();
        var filter = $"(&(objectCategory=person)(objectClass=user)(memberOf={Escape(groupDn)}))";
        var request = new SearchRequest(
            GetSearchBase(),
            filter,
            SearchScope.Subtree,
            "objectGUID", "userPrincipalName", "mail", "displayName", "distinguishedName", "memberOf");

        var response = (SearchResponse)connection.SendRequest(request);
        IReadOnlyList<IdentityUserSnapshot> users = response.Entries.Cast<SearchResultEntry>().Select(Map).ToList();
        return Task.FromResult(users);
    }

    private LdapConnection Bind()
    {
        var o = directoryOptions.Value;
        if (string.IsNullOrWhiteSpace(o.BindUser) || string.IsNullOrWhiteSpace(o.BindPassword))
        {
            throw new InvalidOperationException(
                "AD bind credentials are not configured for identity lookups. Set ActiveDirectory:BindUser and BindPassword from a secrets store.");
        }

        var connection = new LdapConnection(new LdapDirectoryIdentifier(o.Host, o.Port, false, false));
        connection.SessionOptions.SecureSocketLayer = o.UseSsl;
        connection.SessionOptions.ProtocolVersion = 3;
        connection.AuthType = AuthType.Basic;
        connection.Credential = new NetworkCredential(o.BindUser, o.BindPassword);
        connection.Bind();
        return connection;
    }

    private string GetSearchBase()
    {
        var host = directoryOptions.Value.Host;
        // Prefer an explicit base later; for now derive a DC= style guess only when configured via EligibleGroupDn.
        var groupDn = identityOptions.Value.EligibleGroupDn;
        if (!string.IsNullOrWhiteSpace(groupDn))
        {
            var idx = groupDn.IndexOf("DC=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                return groupDn[idx..];
            }
        }

        throw new InvalidOperationException(
            $"Cannot determine LDAP search base. Configure IdentityDirectory:EligibleGroupDn with a DN containing DC= components (host={host}).");
    }

    private static IdentityUserSnapshot Map(SearchResultEntry entry)
    {
        var guidBytes = (byte[])entry.Attributes["objectGUID"][0];
        var objectGuid = new Guid(guidBytes).ToString();
        var upn = GetString(entry, "userPrincipalName");
        var mail = GetString(entry, "mail");
        if (string.IsNullOrWhiteSpace(mail))
        {
            mail = upn;
        }

        var groups = entry.Attributes.Contains("memberOf")
            ? entry.Attributes["memberOf"].GetValues(typeof(string)).Cast<string>().ToArray()
            : [];

        return new IdentityUserSnapshot(
            objectGuid,
            upn,
            mail,
            GetString(entry, "displayName"),
            GetString(entry, "distinguishedName"),
            groups);
    }

    private static string GetString(SearchResultEntry entry, string name) =>
        entry.Attributes.Contains(name) ? entry.Attributes[name][0]?.ToString() ?? string.Empty : string.Empty;

    private static string Escape(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                case '*':
                case '(':
                case ')':
                case '\0':
                    sb.Append('\\').Append(c);
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}
