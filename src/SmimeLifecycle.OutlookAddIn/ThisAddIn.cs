namespace SmimeLifecycle.OutlookAddIn;

/// <summary>
/// Classic Outlook VSTO entry point stub. Package this against Office COM interop in Visual Studio
/// with VSTO workload installed. The add-in delegates to <see cref="IOutlookConfigurationAdapter"/>.
/// </summary>
public sealed class ThisAddIn
{
    private readonly IOutlookConfigurationAdapter _adapter = new ClassicOutlookConfigurationAdapter();

    public OutlookCertificateAvailability OnStartupCheck(string thumbprint) =>
        _adapter.CheckSigningCertificate(thumbprint);
}
