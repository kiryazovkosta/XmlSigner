using DigitalSignaturesClient.Services.Contracts;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System;
using System.IO;

namespace DigitalSignaturesClient.Services
{
    public class SignVerifyEnvelope : ISignVerifyEnvelope
    {
        private readonly IBase64StringService _base64Service;

        public SignVerifyEnvelope(IBase64StringService base64Service)
        {
            _base64Service = base64Service ?? throw new ArgumentNullException(nameof(base64Service));
        }

        // public XmlDocument SignXml(string xmlContent, X509Certificate2 certificate)
        // {
        //     // Load the original XML document
        //     XmlDocument xmlDoc = new XmlDocument();
        //     xmlDoc.PreserveWhitespace = true;
        //     xmlDoc.LoadXml(_base64Service.FromBase64String(xmlContent));
        //
        //     // Wrap the XML inside SignedMessage -> ns2:DataMessage
        //     XmlDocument wrappedXmlDoc = new XmlDocument();
        //     wrappedXmlDoc.PreserveWhitespace = true;
        //     string wrappedContent = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><SignedMessage><ns2:DataMessage xsi:schemaLocation=\"http://www.customs.bg/BgDictionary BgMessages.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ns2=\"http://www.customs.bg/BgDictionary\">{xmlDoc.DocumentElement.OuterXml}</ns2:DataMessage></SignedMessage>";
        //     wrappedXmlDoc.LoadXml(wrappedContent);
        //
        //     // Create a SignedXml object
        //     SignedXml signedXml = new SignedXml(wrappedXmlDoc);
        //     signedXml.SigningKey = certificate.GetRSAPrivateKey();
        //
        //     // Create a reference to be signed (targeting the DataMessage content)
        //     Reference reference = new Reference();
        //     reference.Uri = "";
        //
        //     // Apply the enveloped transform
        //     XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
        //     reference.AddTransform(env);
        //
        //     // Apply the XPath transform to select the content of ns2:DataMessage
        //     var xpath = CreateXPathTransform("/SignedMessage/ns2:DataMessage/*");
        //     reference.AddTransform(xpath);
        //
        //     // Apply the canonicalization transform
        //     XmlDsigC14NTransform c14Transform = new XmlDsigC14NTransform();
        //     reference.AddTransform(c14Transform);
        //
        //     // Add the reference to the SignedXml object
        //     signedXml.AddReference(reference);
        //
        //     // Specify the signature method (RSA-SHA1)
        //     signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
        //     reference.DigestMethod = SignedXml.XmlDsigSHA1Url;
        //
        //
        //     // Include the certificate in the signature
        //     KeyInfo keyInfo = new KeyInfo();
        //     keyInfo.AddClause(new KeyInfoX509Data(certificate));
        //     signedXml.KeyInfo = keyInfo;
        //
        //     // Compute the signature
        //     signedXml.ComputeSignature();
        //
        //     // Get the XML representation of the signature
        //     XmlElement xmlSignature = signedXml.GetXml();
        //
        //     // Append the signature to the SignedMessage element
        //     wrappedXmlDoc.DocumentElement.AppendChild(wrappedXmlDoc.ImportNode(xmlSignature, true));
        //
        //     return wrappedXmlDoc;
        // }

        public string SignXmlMessage(string base64String, X509Certificate2 x509)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                throw new ArgumentNullException(nameof(base64String));
            }

            var xmlMessage = _base64Service.FromBase64String(base64String);

            var xmlDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            xmlDoc.LoadXml(xmlMessage);

            var doc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            XmlNode nodeSignedMessage = doc.AppendChild(doc.CreateElement("SignedMessage"))!;
            XmlNode nodeDataMessage = doc.DocumentElement?.AppendChild(doc.CreateElement("ns2", "DataMessage", "http://www.customs.bg/BgDictionary"))!;
            nodeDataMessage.AppendChild(doc.ImportNode(xmlDoc.DocumentElement!, true));

            XmlAttribute keyAttribute2 = doc.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            keyAttribute2.Value = "http://www.customs.bg/BgDictionary BgMessages.xsd";
            nodeDataMessage.Attributes!.Append(keyAttribute2);

            SignedXml signedXml = new(doc)
            {
                SigningKey = x509.GetRSAPrivateKey(),
            };

            Reference reference = new("");
            XmlDsigEnvelopedSignatureTransform env = new();
            reference.AddTransform(env);
            signedXml.AddReference(reference);

            XmlDsigXPathTransform xPathTransform = CreateXPathTransform("/SignedMessage/ns2:DataMessage/*");
            reference.AddTransform(xPathTransform);

            KeyInfo keyInfo = new();
            //keyInfo.AddClause(new KeyInfoX509Data(x509, X509IncludeOption.EndCertOnly));
            keyInfo.AddClause(new KeyInfoX509Data(x509));
            signedXml.KeyInfo = keyInfo;
            signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

            reference.DigestMethod = SignedXml.XmlDsigSHA1Url;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return XmlFormatting(doc, Encoding.UTF8);
        }

        private static string XmlFormatting(XmlDocument xml, Encoding? encoding = null, Formatting formatting = Formatting.None)
        {
            StreamReader? streamReader = null;
            MemoryStream? memoryStream = null;

            encoding ??= Encoding.UTF8;

            try
            {
                XmlWriter? xmlTextWriter = null;
                XmlWriterSettings settings = new()
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n",
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = encoding
                };

                memoryStream = new MemoryStream();
                xmlTextWriter = XmlWriter.Create(memoryStream);
                xml.Save(xmlTextWriter);

                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                streamReader?.Dispose();
                memoryStream?.Dispose();
            }
        }

        public static XmlDsigXPathTransform CreateXPathTransform(string xPathString)
        {
            // Create a new XmlDocument object.
            XmlDocument doc = new XmlDocument();

            // Create a new XmlElement.
            XmlElement xPathElem = doc.CreateElement("XPath");

            XmlAttribute namespaceDeclaration = doc.CreateAttribute("xmlns", "ns2", "http://www.w3.org/2000/xmlns/");
            namespaceDeclaration.Value = "http://www.customs.bg/BgDictionary";
            xPathElem.Attributes.Append(namespaceDeclaration);

            // Set the element text to the value of the XPath string.
            xPathElem.InnerText = xPathString;

            // Create a new XmlDsigXPathTransform object.
            XmlDsigXPathTransform xForm = new XmlDsigXPathTransform();
            xForm.Algorithm = "http://www.w3.org/TR/1999/REC-xpath-19991116";

            // Load the XPath XML from the element. 
            xForm.LoadInnerXml(xPathElem.SelectNodes(".")!);

            // Return the XML that represents the transform.
            return xForm;
        }
    }
}