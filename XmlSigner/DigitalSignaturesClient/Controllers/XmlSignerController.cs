﻿using DigitalSignaturesClient.Common;
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
    private readonly IBase64StringService _base64Service;

    public XmlSignerController(
        IX509CertificateService certificateService, 
        ISignVerifyEnvelope signEnvelope,
        IBase64StringService base64Service)
    {
        _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
        _signEnvelope = signEnvelope ?? throw new ArgumentNullException(nameof(signEnvelope));
        _base64Service = base64Service ?? throw new ArgumentNullException(nameof(base64Service));
    }

    [HttpPost(Name="Sign")]
    public IActionResult PostSign(SignXmlRequest data)
    {
        switch (data.Certificate.SerialNumber)
        {
            case "111111111111":
                return this.Ok(Constants.FakeBase64String);
            case "000000000000":
                return BadRequest($"Something went wrong when signing the certificate with serial number {data.Certificate.SerialNumber}");
        }
        
        var certificate = _certificateService.GetSignature(data.Certificate.SerialNumber, data.Certificate.NotAfter);
        if (certificate is null)
        {
            return NotFound();
        }

        try
        {
            var response = _signEnvelope.SignXmlMessage(data.Message, certificate);
            return this.Ok(_base64Service.ToBase64String(response));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}