using DigitalSignaturesClient.Services.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace DigitalSignaturesClient.Services
{
    public class SignVerifyEnvelope : ISignVerifyEnvelope
    {
        private readonly IBase64StringService _base64Service;

        public SignVerifyEnvelope(IBase64StringService base64Service)
        {
            _base64Service = base64Service ?? throw new ArgumentNullException(nameof(base64Service));
        }

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

            var nodeDataMessage = doc.AppendChild(doc.CreateElement("DeclarationSubmitRequest"));
            if (xmlDoc.DocumentElement != null)
            {
                nodeDataMessage?.AppendChild(doc.ImportNode(xmlDoc.DocumentElement, true));
            }

            SignedXml signedXml = new SignedXml(doc)
            {
                SigningKey = x509.GetRSAPrivateKey()
            };

            Reference reference = new Reference("");
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            //keyInfo.AddClause(new RSAKeyValue((RSA) x509.PrivateKey));
            keyInfo.AddClause(new KeyInfoX509Data(x509, X509IncludeOption.EndCertOnly));
            signedXml.KeyInfo = keyInfo;
            signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return XmlFormatting(doc, Encoding.UTF8);
        }

        private string XmlFormatting(XmlDocument xml, Encoding? encoding = null, Formatting formatting = Formatting.None)
        {
            StreamReader? streamReader = null;
            MemoryStream? memoryStream = null;

            if (encoding is null)
            {
                encoding = Encoding.UTF8;
            }

            try
            {
                XmlWriter? xmlTextWriter = null;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = "\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                settings.Encoding = encoding;

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
    }
}