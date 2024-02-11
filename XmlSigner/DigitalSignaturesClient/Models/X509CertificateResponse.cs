namespace DigitalSignaturesClient.Models;

public class X509CertificateResponse
{
    public required string Subject { get; set; }
    public required string Issuer { get; set; }
    public required DateTime NotAfter { get; set; }
    public required string SerialNumber { get; set; }
}
