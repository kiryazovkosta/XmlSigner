using DigitalSignaturesClient.Models;
using DigitalSignaturesClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

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
        var certificates = _certificateService.GetValidCertificates()
            .Select(c => new X509CertificateResponse()
            {
                Subject = _certificateService.GetCN(c.Subject),
                Issuer = _certificateService.GetCN(c.Issuer),
                NotAfter = c.NotAfter,
                SerialNumber = c.SerialNumber
            })
            .ToList();

        return this.Ok(certificates);
    }
}