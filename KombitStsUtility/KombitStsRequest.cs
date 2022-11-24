using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using static LanguageExt.Prelude;

namespace KombitStsUtility;

public static class XmlExt
{
    public static XmlElement ToXmlElement(this XElement element)
    {
        var xmlDocument = new XmlDocument();
        using var xmlReader = element.CreateReader();
        xmlDocument.Load(xmlReader);
        return xmlDocument.DocumentElement!;
    }

    public static XmlDocument ToXmlDocument(this XDocument xml)
    {
        var stream = new MemoryStream();
        xml.Save(stream, SaveOptions.DisableFormatting);
        stream.Position = 0;
        var doc = new XmlDocument();
        doc.Load(stream);
        return doc;
    }

    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        var ms = new MemoryStream();
        xmlDocument.Save(ms);
        ms.Position = 0;
        return XDocument.Load(ms);
    }
}

public class KombitStsRequest
{
    public Uri WsAddressingTo { get; }

    public string MunicipalityCvr { get; }

    public X509Certificate2 Certificate { get; }

    public string EndpointEntityId { get; }

    private readonly static XAttribute ValueType = new("ValueType",
        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

    private readonly static XAttribute EncodingType = new("EncodingType",
        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

    private static readonly string
        ActionRef = "_" + Guid.NewGuid(),
        MessageIdRef = "_" + Guid.NewGuid(),
        ToRef = "_" + Guid.NewGuid(),
        ReplyToRef = "_" + Guid.NewGuid(),
        TimestampRef = "TS-" + Guid.NewGuid(),
        BinarySecurityTokenRef = "X509-" + Guid.NewGuid(),
        BodyRef = "_" + Guid.NewGuid();

    private const string SecurityTokenReference = "SecurityTokenReference";

    public KombitStsRequest(int municipalityCvr, X509Certificate2 certificate, string endpointEntityId,
        Uri wsAddressingTo)
        : this(municipalityCvr.ToString(), certificate, endpointEntityId, wsAddressingTo) { }

    public KombitStsRequest(string municipalityCvr, X509Certificate2 certificate, string endpointEntityId,
        Uri wsAddressingTo)
    {
        EndpointEntityId = endpointEntityId;
        WsAddressingTo = wsAddressingTo;
        MunicipalityCvr = municipalityCvr;
        Certificate = certificate;
        Dom = BuildXml();
    }

    private XDocument Dom { get; }

    public XDocument ToXDocument() => Dom;

    public string ToPrettyString() => Dom.ToString();

    public override string ToString() => Dom.ToString(SaveOptions.DisableFormatting);

    private XDocument BuildXml()
    {
        var document = Envelope();
        return XmlSigner.Sign(Certificate, document);
    }

    private XDocument Envelope() => new(new XElement(NameSpaces.xsoap + "Envelope",
        new XAttribute(XNamespace.Xmlns + "soap", NameSpaces.soap), Header(), Body()));

    private XElement Header() =>
        new(NameSpaces.xsoap + "Header",
            new XElement(NameSpaces.xwsa + "Action", WsuId(ActionRef), WsTrustConstants.Wst13IssueAction),
            MessageId(),
            new XElement(NameSpaces.xwsa + "To", WsuId(ToRef), WsAddressingTo),
            new XElement(NameSpaces.xwsa + "ReplyTo", WsuId(ReplyToRef),
                new XElement(NameSpaces.xwsa + "Address", "http://www.w3.org/2005/08/addressing/anonymous")),
            Security(Certificate));

    private static XElement MessageId() =>
        new(NameSpaces.xwsa + "MessageID", WsuId(MessageIdRef),
            "urn:uuid:" + Guid.NewGuid().ToString("D"));

    private static XElement Security(X509Certificate certificate) =>
        new(NameSpaces.xwsse + "Security", new XAttribute(NameSpaces.xsoap + "mustUnderstand", "1"),
            new XAttribute(XNamespace.Xmlns + "wsse", NameSpaces.wsse),
            new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                new XAttribute(NameSpaces.xwsu + "Id", BinarySecurityTokenRef),
                EncodingType,
                ValueType,
                Convert.ToBase64String(certificate.Export(X509ContentType.Cert))),
            Timestamp()
        );

    private static XElement Timestamp()
    {
        var now = DateTime.UtcNow;
        var nowZeroMilli = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        string ToXmlDateTime(DateTime d) => d.ToString("O").Remove(d.ToString("O").Length - 4) + "Z";
        return new XElement(NameSpaces.xwsu + "Timestamp", WsuId(TimestampRef),
           new XElement(NameSpaces.xwsu + "Created", ToXmlDateTime(nowZeroMilli)),
           new XElement(NameSpaces.xwsu + "Expires", ToXmlDateTime(nowZeroMilli.AddMinutes(5))));
    }

    private XElement Body() => new(NameSpaces.xsoap + "Body", WsuId(BodyRef),
        RequestSecurityToken(EndpointEntityId, MunicipalityCvr, Certificate));

    private static IEnumerable<XAttribute> WsuId(string id) =>
        List(new XAttribute(XNamespace.Xmlns + "wsu", NameSpaces.wsu),
            new XAttribute(NameSpaces.xwsu + "Id", id));

    private static XElement RequestSecurityToken(string endpointEntityId, string municipalityCvr,
        X509Certificate certificate) =>
        new(NameSpaces.xwst13 + "RequestSecurityToken", new XAttribute(XNamespace.Xmlns + "wst", NameSpaces.wst13),
            new XElement(NameSpaces.xwst13 + "TokenType", WsseValues.SamlTokenType),
            new XElement(NameSpaces.xwst13 + "RequestType", WsTrustConstants.Wst13IssueRequestType),
            AppliesTo(endpointEntityId),
            Claims(municipalityCvr),
            new XElement(NameSpaces.xwst13 + "KeyType", "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey"),
            new XElement(NameSpaces.xwst13 + "UseKey",
                new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                    new XAttribute(XNamespace.Xmlns + "wsse", NameSpaces.wsse),
                    EncodingType,
                    ValueType,
                    Convert.ToBase64String(certificate.Export(X509ContentType.Cert
                    )))));

    private static XElement AppliesTo(string endpointEntityId) =>
        new(NameSpaces.xwsp + "AppliesTo", new XAttribute(XNamespace.Xmlns + "wsp", NameSpaces.wsp),
            new XElement(NameSpaces.xwsa + "EndpointReference",
                new XAttribute(XNamespace.Xmlns + "adr", NameSpaces.wsa),
                new XElement(NameSpaces.xwsa + "Address", endpointEntityId
                )));

    private static XElement Claims(string municipalityCvr) =>
        new(NameSpaces.xwst13 + "Claims", new XAttribute("Dialect", WsfAuthValues.ClaimsDialect),
            new XElement(NameSpaces.xwsfAuth + "ClaimType",
                new XAttribute(XNamespace.Xmlns + "wsfed", NameSpaces.wsfAuth),
                new XAttribute("Uri", "dk:gov:saml:attribute:CvrNumberIdentifier"),
                new XElement(NameSpaces.xwsfAuth + "Value", municipalityCvr
                )));

    public class XmlSigner : SignedXml
    {
        public static XDocument Sign(X509Certificate2 cert, XDocument doc) => new XmlSigner(doc).Sign(cert);

        private class SecurityTokenReference : KeyInfoClause
        {
            public override XmlElement GetXml() => new XElement(NameSpaces.xwsse + KombitStsRequest.SecurityTokenReference,
                    new XElement(NameSpaces.xwsse + "Reference", new XAttribute("URI", $"#{BinarySecurityTokenRef}"),
                        ValueType))
                .ToXmlElement();

            public override void LoadXml(XmlElement element) => throw new NotImplementedException();
        }

        private XmlDocument Xml { get; }

        private XmlSigner(XDocument xml) : this(xml.ToXmlDocument()) { }

        private XmlSigner(XmlDocument xml) : base(xml) => Xml = xml;

        private XDocument Sign(X509Certificate2 cert)
        {
            List(ActionRef, MessageIdRef, ToRef, ReplyToRef, TimestampRef, BinarySecurityTokenRef, BodyRef)
                .Map(r => '#' + r)
                .Do(r =>
                {
                    var reference = new Reference { Uri = r };
                    reference.AddTransform(new XmlDsigExcC14NTransform());
                    reference.DigestMethod = XmlDsigSHA256Url;
                    AddReference(reference);
                });

            SigningKey = cert.GetRSAPrivateKey();
            SignedInfo.CanonicalizationMethod = new XmlDsigExcC14NTransform().Algorithm;
            SignedInfo.SignatureMethod = XmlDsigRSASHA256Url;
            var ki = new KeyInfo();
            ki.AddClause(new SecurityTokenReference());
            KeyInfo = ki;

            ComputeSignature();

            // Add prefix "ds:" to signature
            var signature = GetXml();
            signature.SetAttribute("xmlns:ds", XmlDsigNamespaceUrl);
            SetSignaturePrefix(signature);

            // Load modified signature back
            LoadXml(signature);

            // this is workaround for overcoming a bug in the library
            SignedInfo.References.Clear();

            // Recompute the signature
            ComputeSignature();
            var recomputedSignature = Convert.ToBase64String(SignatureValue);

            // Replace value of the signature with recomputed one
            ReplaceSignature(signature, recomputedSignature);

            // Append the signature to the XML document.
            const string securityPath = "/soap:Envelope/soap:Header/wsse:Security";
            if (Xml.SelectSingleNode(securityPath, NameSpaces.MakeNsManager(Xml.NameTable)) is not XmlElement xSecurity)
            {
                throw new InvalidOperationException($"No Signature element found in {securityPath}");
            }

            xSecurity.AppendChild(xSecurity.OwnerDocument.ImportNode(signature, true));
            return Xml.ToXDocument();
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            var idElem =
                doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", NameSpaces.MakeNsManager(doc.NameTable)) as
                    XmlElement;
            return idElem ?? base.GetIdElement(doc, id);
        }

        private static void SetSignaturePrefix(XmlNode node)
        {
            if (node.Name == KombitStsRequest.SecurityTokenReference)
            {
                node.Prefix = node.ChildNodes[0]!.Prefix = "wsse";
                return;
            }

            node.Prefix = "ds";
            foreach (XmlNode n in node.ChildNodes)
            {
                SetSignaturePrefix(n);
            }
        }

        private static void ReplaceSignature(XmlElement signature, string newValue)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (signature.OwnerDocument == null) throw new ArgumentException("No owner document", nameof(signature));

            var nsm = new XmlNamespaceManager(signature.OwnerDocument.NameTable);
            nsm.AddNamespace("ds", XmlDsigNamespaceUrl);

            var signatureValue = signature.SelectSingleNode("ds:SignatureValue", nsm);
            if (signatureValue == null)
                throw new Exception("Signature does not contain 'ds:SignatureValue'");

            signatureValue.InnerXml = newValue;
        }
    }
}