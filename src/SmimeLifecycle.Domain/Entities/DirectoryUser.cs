using SmimeLifecycle.Domain.Common;

namespace SmimeLifecycle.Domain.Entities;

public sealed class DirectoryUser : Entity
{
    /// <summary>Active Directory objectGUID (string form) for the user.</summary>
    public string DirectoryObjectGuid { get; private set; } = string.Empty;
    public string UserPrincipalName { get; private set; } = string.Empty;
    public string Mail { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? DistinguishedName { get; private set; }
    public bool IsEligible { get; private set; }
    public string? EligibilityReason { get; private set; }

    private DirectoryUser()
    {
    }

    public static DirectoryUser Create(
        string directoryObjectGuid,
        string userPrincipalName,
        string mail,
        string displayName,
        string? distinguishedName,
        bool isEligible,
        string? eligibilityReason)
    {
        return new DirectoryUser
        {
            DirectoryObjectGuid = directoryObjectGuid,
            UserPrincipalName = userPrincipalName,
            Mail = mail,
            DisplayName = displayName,
            DistinguishedName = distinguishedName,
            IsEligible = isEligible,
            EligibilityReason = eligibilityReason
        };
    }

    public void UpdateEligibility(bool isEligible, string? reason)
    {
        IsEligible = isEligible;
        EligibilityReason = reason;
        Touch();
    }

    public void UpdateDirectoryBinding(string? distinguishedName)
    {
        DistinguishedName = distinguishedName;
        Touch();
    }
}
