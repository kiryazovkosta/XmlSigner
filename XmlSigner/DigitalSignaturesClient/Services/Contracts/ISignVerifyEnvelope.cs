using System.Security.Cryptography.X509Certificates;

namespace DigitalSignaturesClient.Services.Contracts;

public interface ISignVerifyEnvelope
{
    string SignXmlMessage(string xmlMessage, X509Certificate2 x509);
}
