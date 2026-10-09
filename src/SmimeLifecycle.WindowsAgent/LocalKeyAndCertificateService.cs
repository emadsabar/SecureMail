using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SmimeLifecycle.WindowsAgent;

[SupportedOSPlatform("windows")]
public sealed class LocalKeyAndCertificateService
{
    public (string CsrPem, string KeyName) GenerateNonExportableCsr(string subject, string email)
    {
        var keyName = $"smime-lifecycle-{Guid.NewGuid():N}";
        var creation = new CngKeyCreationParameters
        {
            ExportPolicy = CngExportPolicies.None,
            KeyUsage = CngKeyUsages.Signing | CngKeyUsages.Decryption,
            Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
            KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey
        };
        creation.Parameters.Add(new CngProperty("Length", BitConverter.GetBytes(2048), CngPropertyOptions.None));

        using var cngKey = CngKey.Create(CngAlgorithm.Rsa, keyName, creation);
        using var rsa = new RSACng(cngKey);
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            [new Oid("1.3.6.1.5.5.7.3.4"), new Oid("1.3.6.1.5.5.7.3.2")], true));
        var san = new SubjectAlternativeNameBuilder();
        san.AddEmailAddress(email);
        request.CertificateExtensions.Add(san.Build());

        var csr = request.CreateSigningRequest();
        var pem = PemEncoding.Write("CERTIFICATE REQUEST", csr);
        return (new string(pem), keyName);
    }

    public string InstallIssuedCertificate(string publicPem, string keyName)
    {
        using var publicCert = X509Certificate2.CreateFromPem(publicPem);
        using var cngKey = CngKey.Open(keyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        using var rsa = new RSACng(cngKey);
        using var withKey = publicCert.CopyWithPrivateKey(rsa);
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        store.Add(withKey);
        return withKey.Thumbprint;
    }

    public bool IsCertificateInCurrentUserMy(string thumbprint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        return store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false).Count > 0;
    }
}
