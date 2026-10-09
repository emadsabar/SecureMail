using SmimeLifecycle.Domain.Common;

namespace SmimeLifecycle.Domain.Entities;

public sealed class DirectoryUser : Entity
{
    public string EntraObjectId { get; private set; } = string.Empty;
    public string UserPrincipalName { get; private set; } = string.Empty;
    public string Mail { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? OnPremisesDistinguishedName { get; private set; }
    public bool IsEligible { get; private set; }
    public string? EligibilityReason { get; private set; }

    private DirectoryUser()
    {
    }

    public static DirectoryUser Create(
        string entraObjectId,
        string userPrincipalName,
        string mail,
        string displayName,
        string? onPremisesDistinguishedName,
        bool isEligible,
        string? eligibilityReason)
    {
        return new DirectoryUser
        {
            EntraObjectId = entraObjectId,
            UserPrincipalName = userPrincipalName,
            Mail = mail,
            DisplayName = displayName,
            OnPremisesDistinguishedName = onPremisesDistinguishedName,
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
        OnPremisesDistinguishedName = distinguishedName;
        Touch();
    }
}
