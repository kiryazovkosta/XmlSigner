using DigitalSignaturesClient.Services.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignaturesClient.Services;

internal class X509CertificateService : IX509CertificateService
{
    public X509Certificate2? GetSignature(string serialNumber, DateTime notAfter)
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        var validCertificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);
        foreach (var certificate in validCertificates)
        {
            if (certificate.SerialNumber == serialNumber && certificate.NotAfter == notAfter)
            {
                return certificate;
            }
        }

        return null;
    }

    public string GetCN(string property)
    {
        try
        {
            var i = property.IndexOf("CN=", StringComparison.Ordinal);
            var j = property.IndexOf(",", i, StringComparison.Ordinal);
            if (i < 0)
            {
                return string.Empty;
            }

            if (j - i - 3 > 0)
            {
                return property.Substring(i + 3, j - i - 3);
            }

            return property[(i + 3)..];
        }
        catch (Exception)
        {
            return property;
        }
    }

    public IEnumerable<X509Certificate2> GetValidCertificates()
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        return store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false)
            .Cast<X509Certificate2>();
    }
}
