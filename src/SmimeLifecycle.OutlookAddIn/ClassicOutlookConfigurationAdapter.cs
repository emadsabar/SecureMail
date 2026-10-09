using System.Security.Cryptography.X509Certificates;
using SmimeLifecycle.Domain.Enums;

namespace SmimeLifecycle.OutlookAddIn;

public sealed class ClassicOutlookConfigurationAdapter : IOutlookConfigurationAdapter
{
    public OutlookClientProfile Profile => OutlookClientProfile.ClassicOutlookWindows;
    public bool IsSupported => true;

    public OutlookCertificateAvailability CheckSigningCertificate(string thumbprint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var found = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
        return found.Count > 0
            ? new OutlookCertificateAvailability(true, "Certificate is available to classic Outlook via CurrentUser\\My.")
            : new OutlookCertificateAvailability(false, "Certificate is not in CurrentUser\\My.");
    }

    public OutlookConfigurationResult ConfigureSigning(string thumbprint)
    {
        var availability = CheckSigningCertificate(thumbprint);
        if (!availability.PresentInStore)
        {
            return new OutlookConfigurationResult(false, availability.Detail ?? "Certificate missing.");
        }

        return new OutlookConfigurationResult(
            true,
            "Certificate is present for classic Outlook. Prefer Group Policy/Intune for ForceDefaultSMIMESettings and related documented values rather than unsupported Outlook object-model writes.");
    }
}

public sealed class UnsupportedOutlookAdapter(OutlookClientProfile profile) : IOutlookConfigurationAdapter
{
    public OutlookClientProfile Profile { get; } = profile;
    public bool IsSupported => false;

    public OutlookCertificateAvailability CheckSigningCertificate(string thumbprint) =>
        new(false, $"{Profile} is outside Phase 1 support.");

    public OutlookConfigurationResult ConfigureSigning(string thumbprint) =>
        new(false, $"{Profile} requires a dedicated compatibility profile and validation.");
}
