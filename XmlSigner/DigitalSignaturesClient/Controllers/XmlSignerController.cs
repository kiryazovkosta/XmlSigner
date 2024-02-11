using DigitalSignaturesClient.Models;
using DigitalSignaturesClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DigitalSignaturesClient.Controllers;

[ApiController]
[Route("api/xmlSigner")]
public class XmlSignerController : ControllerBase
{
    private readonly IX509CertificateService _certificateService;
    private readonly ISignVerifyEnvelope _signEnvelope;

    public XmlSignerController(
        IX509CertificateService certificateService, 
        ISignVerifyEnvelope signEnvelope)
    {
        _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        _signEnvelope = signEnvelope ?? throw new ArgumentNullException(nameof(signEnvelope));
    }

    [HttpPost]
    public IActionResult PostSign(SignXmlRequest data)
    {
        var certificate = _certificateService.GetSignature(data.Certificate.SerialNumber, data.Certificate.NotAfter);
        if (certificate is null)
        {
            return NotFound();
        }

        try
        {
            data.Message = _signEnvelope.SignXmlMessage(data.Message, certificate);
            return this.Ok(data);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}