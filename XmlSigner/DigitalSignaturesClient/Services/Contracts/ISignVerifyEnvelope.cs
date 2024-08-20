using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace DigitalSignaturesClient.Services.Contracts;

public interface ISignVerifyEnvelope
{
    string SignXmlMessage(string xmlMessage, X509Certificate2 x509);

    //XmlDocument SignXml(string xmlMessage, X509Certificate2 certificate);
}
