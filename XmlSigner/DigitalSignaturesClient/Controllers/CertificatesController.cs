using DigitalSignaturesClient.Models;
using DigitalSignaturesClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignaturesClient.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly IX509CertificateService _certificateService;

    public CertificatesController(IX509CertificateService certificateService)
    {
        _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
    }

    [HttpGet]
    public IActionResult GetCertificates()
    {
        var certificates = new List<X509CertificateResponse>();

        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        foreach (var certificate in store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false))
        {
            certificates.Add(new X509CertificateResponse()
            {
                Subject = _certificateService.GetCN(certificate.Subject),
                Issuer = _certificateService.GetCN(certificate.Issuer),
                NotAfter = certificate.NotAfter,
                SerialNumber = certificate.SerialNumber
            });
        }

        return this.Ok(certificates);
    }
}
