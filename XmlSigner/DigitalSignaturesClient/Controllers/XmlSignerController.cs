using DigitalSignaturesClient.Models;
using DigitalSignaturesClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DigitalSignaturesClient.Controllers;

[ApiController]
[Route("api/xmlSigner")]
public class XmlSignerController : ControllerBase
{
    private readonly IX509CertificateService _certificateService;
    private readonly ISignVerifyEnvelope _signEnvelope;
    private readonly IBase64StringService _base64Service;
    private readonly ILogger<XmlSignerController> _logger;

    public XmlSignerController(
        IX509CertificateService certificateService, 
        ISignVerifyEnvelope signEnvelope,
        IBase64StringService base64Service,
        ILogger<XmlSignerController> logger)
    {
        _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        _signEnvelope = signEnvelope ?? throw new ArgumentNullException(nameof(signEnvelope));
        _base64Service = base64Service ?? throw new ArgumentNullException(nameof(base64Service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost(Name="Sign")]
    public async Task<ActionResult> PostSign(SignXmlRequest data)
    {
        switch (data.Certificate.SerialNumber)
        {
            case "111111111111":
                return this.Ok(Common.Constants.FakeBase64String);
            case "000000000000":
                return BadRequest($"Something went wrong when signing the certificate with serial number {data.Certificate.SerialNumber}");
        }
        
        var certificate = _certificateService.GetSignature(data.Certificate.SerialNumber, data.Certificate.NotAfter);
        if (certificate is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                type: $"/errors/CertificateNotFound",
                title: "Certificate Not Found",
                detail: "There is no certificate with provided SerialNumber.",
                instance: HttpContext.Request.Path);
        }

        try
        {
            var response = _signEnvelope.SignXmlMessage(data.Message, certificate);
            return this.Ok(_base64Service.ToBase64String(response));
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "/errors/SignXmlError",
                Title = "SignXmlError",
                Detail = "There is a problem with signing of provided xml.",
                Instance = HttpContext.Request.Path
            };

            problemDetails.Extensions["exceptionMessage"] = ex.Message;
            problemDetails.Extensions["exceptionType"] = ex.GetType().FullName;

            _logger.LogError(ex, ex.Message);

            return BadRequest(problemDetails);
        }
    }
}