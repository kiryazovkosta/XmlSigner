using System.Security.Cryptography.X509Certificates;

namespace DigitalSignaturesClient.Services.Contracts;

public interface IX509CertificateService
{
    X509Certificate2? GetSignature(string serialNumber, DateTime notAfter);
    string GetCN(string property);
}
