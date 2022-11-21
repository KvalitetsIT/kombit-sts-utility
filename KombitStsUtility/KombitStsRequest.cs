using dk.nsi.seal.Model.Constants;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.Model;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Serialization;
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

    public string EndpointReference { get; }

    private readonly static XAttribute ValueType = new("ValueType",
        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

    private readonly static XAttribute EncodingType = new("EncodingType",
        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

    private static readonly string
        actionRef = "_" + Guid.NewGuid(),
        messageIdRef = "_" + Guid.NewGuid(),
        toRef = "_" + Guid.NewGuid(),
        replyToRef = "_" + Guid.NewGuid(),
        timestampRef = "TS-" + Guid.NewGuid(),
        binarySecurityTokenRef = "X509-" + Guid.NewGuid(),
        bodyRef = "_" + Guid.NewGuid(),
        securityTokenReference = "SecurityTokenReference";


    private class XmlSigner : SignedXml
    {
        private class SecurityTokenReference : KeyInfoClause
        {
            public override XmlElement GetXml() => new XElement(NameSpaces.xwsse + securityTokenReference,
                    new XElement(NameSpaces.xwsse + "Reference", new XAttribute("URI", $"#{binarySecurityTokenRef}"),
                        ValueType))
                .ToXmlElement();

            public override void LoadXml(XmlElement element) => throw new NotImplementedException();
        }

        private XmlDocument Xml { get; }

        private XmlSigner(XDocument xml) : this(xml.ToXmlDocument()) { }

        private XmlSigner(XmlDocument xml) : base(xml) => Xml = xml;

        private XDocument Sign(X509Certificate2 cert)
        {
            List(actionRef, messageIdRef, toRef, replyToRef, timestampRef, binarySecurityTokenRef, bodyRef)
                .Map(s => "#" + s)
                .Iter(s =>
                {
                    var reference = new Reference { Uri = s };
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
            if (node.Name == securityTokenReference)
            {
                node.Prefix = node.ChildNodes[0].Prefix = "wsse";
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

        public static XDocument Sign(X509Certificate2 cert, XDocument doc) => new XmlSigner(doc).Sign(cert);
    }

    public KombitStsRequest(int municipalityCvr, X509Certificate2 certificate, string endpointReference,
        Uri wsAddressingTo)
        : this(municipalityCvr.ToString(), certificate, endpointReference, wsAddressingTo) { }

    public KombitStsRequest(string municipalityCvr, X509Certificate2 certificate, string endpointReference,
        Uri wsAddressingTo)
    {
        EndpointReference = endpointReference;
        WsAddressingTo = wsAddressingTo;
        MunicipalityCvr = municipalityCvr;
        Certificate = certificate;
        Dom = BuildXml();
    }

    private XDocument Dom { get; }

    public XDocument ToDom() => Dom;

    public string ToPrettyString() => Dom.ToString();

    public override string ToString() => Dom.ToString(SaveOptions.DisableFormatting);

    private XDocument BuildXml()
    {
        var document = Envelope();
        return XmlSigner.Sign(Certificate, document);
    }

    private XDocument Envelope() => new(new XElement(NameSpaces.xsoap + "Envelope",
        new XAttribute(XNamespace.Xmlns + "soap", NameSpaces.soap), Header(), Body()));

    private XElement Header()
    {
        var header =
            new XElement(NameSpaces.xsoap + "Header",
                new XElement(NameSpaces.xwsa + "Action", WsuId(actionRef), WsTrustConstants.Wst13IssueAction),
                MessageId(),
                new XElement(NameSpaces.xwsa + "To", WsuId(toRef), WsAddressingTo),
                new XElement(NameSpaces.xwsa + "ReplyTo", WsuId(replyToRef),
                    new XElement("Address", "http://www.w3.org/2005/08/addressing/anonymous")));
        
        var security = AddWsSecurityHeader(Certificate);
        header.Add(security);
        AddWsuTimestamp(security);
        return header;
    }

    private static XElement MessageId() => 
        new(NameSpaces.xwsa + "MessageID", WsuId(messageIdRef),
        "urn:uuid:" + Guid.NewGuid().ToString("D"));

    private static IEnumerable<XAttribute> WsuId(string id) =>
        List(new XAttribute(XNamespace.Xmlns + "wsu", NameSpaces.wsu),
            new XAttribute(NameSpaces.xwsu + "Id", id));

    private static XElement AddWsSecurityHeader(X509Certificate certificate) =>
        XmlUtil.CreateElement(WsseTags.Security,
            new XAttribute(NameSpaces.xsoap + "mustUnderstand", "1"),
            new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                new XAttribute(NameSpaces.xwsu + "Id", binarySecurityTokenRef),
                EncodingType,
                ValueType,
                Convert.ToBase64String(certificate.Export(X509ContentType.Cert
                ))));

    private static void AddWsuTimestamp(XContainer securityHeader)
    {
        var timestamp = XmlUtil.CreateElement(WsuTags.Timestamp);
        securityHeader.Add(timestamp);
        var now = DateTime.UtcNow;
        var nowZeroMilli = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        string ToXmlDateTime(DateTime d) => d.ToString("O").Remove(d.ToString("O").Length - 4) + "Z";
        var created = XmlUtil.CreateElement(WsuTags.Created, ToXmlDateTime(nowZeroMilli));
        var expires = XmlUtil.CreateElement(WsuTags.Expires, ToXmlDateTime(nowZeroMilli.AddMinutes(5)));
        timestamp.Add(new XAttribute(NameSpaces.xwsu + "Id", timestampRef), created, expires);
    }
    
    private XElement Body() => new(NameSpaces.xsoap + "Body", new XAttribute(NameSpaces.xwsu + "Id", bodyRef),
        RequestSecurityToken(EndpointReference, MunicipalityCvr, Certificate));

    private static XElement RequestSecurityToken(string endpointReference, string municipalityCvr,
        X509Certificate certificate) =>
        new(NameSpaces.xwst13 + "RequestSecurityToken",
            new XElement(NameSpaces.xwst13 + "TokenType", WsseValues.SamlTokenType),
            new XElement(NameSpaces.xwst13 + "RequestType", WsTrustConstants.Wst13IssueRequestType),
            EndpointReferenceElement(endpointReference),
            Claims(municipalityCvr),
            new XElement(NameSpaces.xwst13 + "KeyType", "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey"),
            new XElement(NameSpaces.xwst13 + "UseKey",
                new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                    EncodingType,
                    ValueType,
                    Convert.ToBase64String(certificate.Export(X509ContentType.Cert
                    )))));

    private static XElement EndpointReferenceElement(string endpointReference) =>
        XmlUtil.CreateElement(WspTags.AppliesTo,
            XmlUtil.CreateElement(WsaTags.EndpointReference,
                XmlUtil.CreateElement(WsaTags.Address, endpointReference
                )));

    private static XElement Claims(string municipalityCvr) =>
        new(NameSpaces.xwst13 + "Claims", new XAttribute("Dialect", WsfAuthValues.ClaimsDialect),
            new XElement(NameSpaces.xwsfAuth + "ClaimType",
                new XAttribute("Uri", "dk:gov:saml:attribute:CvrNumberIdentifier"),
                new XElement(NameSpaces.xwsfAuth + "Value", municipalityCvr
                )));
}