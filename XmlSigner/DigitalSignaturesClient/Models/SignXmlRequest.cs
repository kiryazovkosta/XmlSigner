namespace DigitalSignaturesClient.Models;

public class SignXmlRequest
{
    public required X509CertificateResponse Certificate { get; set; }
    public required string Message { get; set; }
}
