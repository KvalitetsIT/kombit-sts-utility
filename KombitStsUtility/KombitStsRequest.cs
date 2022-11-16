﻿using dk.nsi.seal.Model.Constants;
using LanguageExt;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.Model;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Collections.Immutable;

namespace KombitStsUtility;

public class KombitStsRequest
{
    public Option<string> WsAddressingTo { get; init; }

    public X509Certificate2 Certificate { get; }

    public string Audience { get; }

    private readonly static XAttribute ValueType = new("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

    private readonly static XAttribute EncodingType = new("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

    public string Action { get; } = WsTrustConstants.Wst13IssueAction;

    private class XmlSigner : SignedXml
    {
        private XmlDocument Xml { get; }

        private XmlSigner(XDocument xml) : this(StreamToXml(XDocToStream(xml))) { }

        private XmlSigner(XmlDocument xml) : base(xml) => Xml = xml;

        private XmlDocument Sign(X509Certificate2 cert)
        {
            var refnames = new[] { "#messageID", "#action", "#timestamp", "#body" };

            foreach (var s in refnames)
            {
                var reference = new Reference { Uri = s };
                reference.AddTransform(new XmlDsigExcC14NTransform());
                reference.DigestMethod = XmlDsigSHA1Url;
                AddReference(reference);
            }

            SigningKey = cert.GetRSAPrivateKey();
            SignedInfo.CanonicalizationMethod = new XmlDsigExcC14NTransform().Algorithm;
            SignedInfo.SignatureMethod = XmlDsigRSASHA1Url;

            ComputeSignature();

            XmlElement signaelm = GetXml();
            
            const string securityPath = "/soap:Envelope/soap:Header/wsse:Security";
            if (Xml.SelectSingleNode(securityPath, NameSpaces.MakeNsManager(Xml.NameTable)) is not XmlElement xSecurity)
            { throw new InvalidOperationException($"No Signature element found in {securityPath}"); }
            xSecurity.AppendChild(xSecurity.OwnerDocument.ImportNode(signaelm, true));

            return Xml;
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            var idElem = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", NameSpaces.MakeNsManager(doc.NameTable)) as XmlElement;
            var tid = idElem ?? base.GetIdElement(doc, id);
            return tid;
        }

        private static XmlDocument StreamToXml(Stream stream)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(stream);
            return doc;
        }

        private static Stream XDocToStream(XDocument xml)
        {
            var ms = new MemoryStream();
            xml.Save(ms, SaveOptions.DisableFormatting);
            ms.Position = 0;
            return ms;
        }

        public static XDocument Sign(X509Certificate2 cert, XDocument doc) => XDocument.Parse(new XmlSigner(doc).Sign(cert).OuterXml, LoadOptions.PreserveWhitespace);
    }

    public KombitStsRequest(X509Certificate2 certificate, string audience)
    {
        Audience = audience;
        Certificate = certificate;
    }

    
    private static XElement RequestSecurityToken(string audience, X509Certificate2 certificate) => new(NameSpaces.xtrust + "RequestSecurityToken",
            new XElement(NameSpaces.xtrust + "TokenType", WsseValues.SamlTokenType),
            new XElement(NameSpaces.xtrust + "RequestType", WsTrustConstants.Wst13IssueRequestType),
            AudienceElement(audience),
            Claims(),
            new XElement(NameSpaces.xtrust + "KeyType", "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey"),
            new XElement(NameSpaces.xtrust + "UseKey",
                new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                    EncodingType,
                    ValueType,
                    Convert.ToBase64String(certificate.Export(X509ContentType.Cert))
            )));

    private static XElement Claims() => new(NameSpaces.xtrust + "Claims",
            new XAttribute("Dialect", WsfAuthValues.ClaimsDialect),
            new XElement(NameSpaces.xwsfAuth + "ClaimType",
                new XAttribute("Uri", "dk:gov:saml:attribute:CvrNumberIdentifier"),
                new XElement(NameSpaces.xwsfAuth + "Value", 38163264))
            );

    private static XElement AudienceElement(string audience)
    {
        var doc = new XDocument();
        var appliesTo = XmlUtil.CreateElement(WspTags.AppliesTo);
        doc.Add(appliesTo);

        var endpointReference = XmlUtil.CreateElement(WsaTags.EndpointReference);
        appliesTo.Add(endpointReference);

        var address = XmlUtil.CreateElement(WsaTags.Address);
        endpointReference.Add(address);
        address.Value = audience;

        return doc.Root;
    }

    private void AddExtraHeaders(XElement header)
    {
        WsAddressingTo.IfSome(to => header.Add(new XElement(NameSpaces.xwsa + "To", to)));
        header.Add(new XElement(NameSpaces.xwsa + "ReplyTo", new XElement("Address", "http://www.w3.org/2005/08/addressing/anonymous")));
    }

    private XDocument Build()
    {
        var document = Envelope();
        SealUtilities.CheckAndSetSamlDsPreFix(document);
        NameSpaces.SetMissingNamespaces(document);
        return XmlSigner.Sign(Certificate, document);
    }

    private XDocument Envelope()
    {
        var root = new XElement(NameSpaces.xsoap + "Envelope");
        AppendToRoot(root);
        return new XDocument(root);
    }

    private void AppendToRoot(XElement root)
    {
        // HACK: Needs to write body first, or the assertion validation will fail after signing the message in the header.
        // hash values for assertion will change if header is not last?
        root.Add(Body());
        root.Add(Header());
    }

    private XElement Body() => new(NameSpaces.xsoap + "Body", new XAttribute(NameSpaces.xwsu + "Id", "body"),
                               RequestSecurityToken(Audience, Certificate));

    private XElement Header()
    {
        var header = new XElement(NameSpaces.xsoap + "Header");
        AddHeaderContent(header);
        return header;
    }

    private void AddHeaderContent(XElement header)
    {
        var action = XmlUtil.CreateElement(WsaTags.Action);
        action.Add(new XAttribute(NameSpaces.xwsu + "Id", "action"));
        header.Add(action);
        action.Value = Action;

        var messageId = XmlUtil.CreateElement(WsaTags.MessageId);
        messageId.Add(new XAttribute(NameSpaces.xwsu + "Id", "messageID"));
        header.Add(messageId);
        messageId.Value = "urn:uuid:" + Guid.NewGuid().ToString("D");

        AddExtraHeaders(header);
        var security = AddWsSecurityHeader(Certificate);
        header.Add(security);
        AddWsuTimestamp(security);
    }

    private static void AddWsuTimestamp(XElement securityHeader)
    {
        var timestamp = XmlUtil.CreateElement(WsuTags.Timestamp);
        securityHeader.Add(timestamp);
        var now = DateTime.UtcNow;
        var created = XmlUtil.CreateElement(WsuTags.Created, now.FormatDateTimeXml());
        var expires = XmlUtil.CreateElement(WsuTags.Expires, now.AddMinutes(5).FormatDateTimeXml());
        timestamp.Add(new XAttribute(NameSpaces.xwsu + "Id", "timestamp"), created, expires);
    }

    private static XElement AddWsSecurityHeader(X509Certificate2 certificate) => 
        XmlUtil.CreateElement(WsseTags.Security,
            new XAttribute(NameSpaces.xsoap + "mustUnderstand", "1"),
            new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                               EncodingType,
                               ValueType,
                               Convert.ToBase64String(certificate.Export(X509ContentType.Cert))));

    public XDocument ToXml() => Build();

    public override string ToString() => ToXml().ToString();
}