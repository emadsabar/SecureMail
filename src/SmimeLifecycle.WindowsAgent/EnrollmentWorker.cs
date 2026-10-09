using Microsoft.Extensions.Options;

namespace SmimeLifecycle.WindowsAgent;

public sealed class EnrollmentWorker(
    ILogger<EnrollmentWorker> logger,
    LifecycleApiClient api,
    LocalKeyAndCertificateService crypto,
    IOptions<AgentOptions> options) : BackgroundService
{
    private Guid _deviceId;
    private readonly Dictionary<Guid, string> _keyNames = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var o = options.Value;
        _deviceId = await api.RegisterAsync(new
        {
            o.DirectoryUserId,
            Hostname = Environment.MachineName,
            OsVersion = Environment.OSVersion.VersionString,
            AgentVersion = "1.0.0",
            o.AdComputerObjectGuid,
            o.EnrollmentSecret
        }, stoppingToken);
        logger.LogInformation("Registered Windows agent device {DeviceId}", _deviceId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Enrollment loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, o.PollSeconds)), stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        var pending = await api.GetPendingAsync(_deviceId, cancellationToken);
        if (pending is not null && !_keyNames.ContainsKey(pending.EnrollmentId))
        {
            var (csr, keyName) = crypto.GenerateNonExportableCsr(pending.Subject, pending.Mail);
            _keyNames[pending.EnrollmentId] = keyName;
            await api.SubmitCsrAsync(new
            {
                pending.EnrollmentId,
                DeviceId = _deviceId,
                CsrPem = csr
            }, cancellationToken);
            logger.LogInformation("Submitted CSR for enrollment {EnrollmentId}", pending.EnrollmentId);
        }

        foreach (var enrollmentId in _keyNames.Keys.ToList())
        {
            var issued = await api.GetIssuedAsync(enrollmentId, cancellationToken);
            if (issued is null)
            {
                continue;
            }

            var thumbprint = crypto.InstallIssuedCertificate(issued.PublicCertificatePem, _keyNames[enrollmentId]);
            await api.ReportInstalledAsync(new
            {
                issued.CertificateId,
                DeviceId = _deviceId,
                Thumbprint = thumbprint
            }, cancellationToken);

            var present = crypto.IsCertificateInCurrentUserMy(thumbprint);
            await api.ReportOutlookAsync(new
            {
                DirectoryUserId = options.Value.DirectoryUserId,
                DeviceId = _deviceId,
                Profile = 0,
                Status = present ? 1 : 4,
                SigningThumbprint = thumbprint,
                Detail = present
                    ? "Classic Outlook can use the certificate from CurrentUser\\My. Signing toggle remains an Outlook adapter concern."
                    : "Certificate not found in CurrentUser\\My."
            }, cancellationToken);
            _keyNames.Remove(enrollmentId);
            logger.LogInformation("Installed certificate {Thumbprint}", thumbprint);
        }
    }
}
