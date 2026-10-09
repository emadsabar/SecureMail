using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.OutlookAddIn;

/// <summary>
/// Replaceable Outlook configuration adapter. Phase 1 targets classic Outlook for Windows.
/// Other clients must keep an explicit compatibility profile and are not claimed as supported.
/// </summary>
public interface IOutlookConfigurationAdapter
{
    OutlookClientProfile Profile { get; }

    bool IsSupported { get; }

    OutlookCertificateAvailability CheckSigningCertificate(string thumbprint);

    /// <summary>
    /// Applies documented Outlook signing defaults where the host permits it.
    /// Classic Outlook commonly uses the default signing certificate in CurrentUser\My
    /// plus optional registry/policy. This method must not invent undocumented COM hacks.
    /// </summary>
    OutlookConfigurationResult ConfigureSigning(string thumbprint);
}

public sealed record OutlookCertificateAvailability(bool PresentInStore, string? Detail);

public sealed record OutlookConfigurationResult(bool Applied, string Detail);
