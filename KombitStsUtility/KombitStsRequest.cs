using dk.nsi.seal.Model.Constants;
using LanguageExt;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.Model;
using System.Security.Cryptography.X509Certificates;
using LanguageExt.Pretty;
using System.Runtime.ConstrainedExecution;

namespace KombitStsUtility;

public class KombitStsRequest
{
    public Option<string> WsAddressingTo { get; init; }

    public X509Certificate2 Certificate { get; }

    public string Audience { get; }

    public string BinarySecurityToken { get; }

    private readonly static XAttribute ValueType = new("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

    private readonly static XAttribute EncodingType = new("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

    public string Action { get; } = WsTrustConstants.Wst13IssueAction;

    public KombitStsRequest(X509Certificate2 certificate, string audience, string binarySecurityToken)
    {
        Audience = audience;
        BinarySecurityToken = binarySecurityToken;
        Certificate = certificate;
    }

    protected void AddBodyContent(XElement body) => body.Add(RequestSecurityToken(Audience, BinarySecurityToken));


    private static XElement RequestSecurityToken(string audience, string binarySecurityToken) => new(NameSpaces.xtrust + "RequestSecurityToken",
            new XElement(NameSpaces.xtrust + "TokenType", WsseValues.SamlTokenType),
            new XElement(NameSpaces.xtrust + "RequestType", WsTrustConstants.Wst13IssueRequestType),
            AudienceElement(audience),
            Claims(),
            new XElement(NameSpaces.xtrust + "KeyType", "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey"),
            new XElement(NameSpaces.xtrust + "UseKey",
                new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                    EncodingType,
                    ValueType,
                    binarySecurityToken
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

    protected void AddExtraHeaders(XElement header)
    {
        WsAddressingTo.IfSome(to => header.Add(new XElement(NameSpaces.xwsa + "To", to)));
        header.Add(new XElement(NameSpaces.xwsa + "ReplyTo", new XElement("Address", "http://www.w3.org/2005/08/addressing/anonymous")));
    }

    private XDocument Build()
    {
        var document = CreateDocument();
        SealUtilities.CheckAndSetSamlDsPreFix(document);
        NameSpaces.SetMissingNamespaces(document);
        return SealSignedXml.Sign(Certificate, document);
    }

    private XDocument CreateDocument()
    {
        var doc = new XDocument();
        var root = XmlUtil.CreateElement(SoapTags.Envelope);
        doc.Add(root);
        AppendToRoot(root);
        return doc;
    }

    private void AppendToRoot(XElement root)
    {
        // HACK: Needs to write body first, or the assertion validation will fail after signing the message in the header.
        // hash values for assertion will change if header is not last?
        AppendBody(root);
        AppendHeader(root);
    }

    private void AppendBody(XElement envelope)
    {
        var body = XmlUtil.CreateElement(SoapTags.Body);
        body.Add(new XAttribute(NameSpaces.xwsu + "Id", "body"));

        envelope.Add(body);
        AddBodyContent(body);
    }

    private void AppendHeader(XElement envelope)
    {
        var header = XmlUtil.CreateElement(SoapTags.Header);
        envelope.Add(header);
        AddHeaderContent(header);
    }

    protected void AddHeaderContent(XElement header)
    {
        var action = XmlUtil.CreateElement(WsaTags.Action);
        action.Add(new XAttribute("mustUnderstand", "1"),
                   new XAttribute(NameSpaces.xwsu + "Id", "action"));
        header.Add(action);
        action.Value = Action;

        var messageId = XmlUtil.CreateElement(WsaTags.MessageId);
        messageId.Add(new XAttribute(NameSpaces.xwsu + "Id", "messageID"));
        header.Add(messageId);
        messageId.Value = "urn:uuid:" + Guid.NewGuid().ToString("D");

        AddExtraHeaders(header);
        var security = AddWsSecurityHeader(BinarySecurityToken);
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

    private static XElement AddWsSecurityHeader(string binarySecurityToken)
    {
        var securityHeader = XmlUtil.CreateElement(WsseTags.Security);
        securityHeader.Add(new XElement(NameSpaces.xwsse + "BinarySecurityToken",
                               EncodingType,
                               ValueType,
                               binarySecurityToken)
            );
        return securityHeader;
    }

    protected void AddHeader(XElement header)
    {
        var action = XmlUtil.CreateElement(WsaTags.Action);
        action.Add(new XAttribute("mustUnderstand", "1"),
                   new XAttribute(NameSpaces.xwsu + "Id", "action"));
        header.Add(action);
        action.Value = Action;

        var messageId = XmlUtil.CreateElement(WsaTags.MessageId);
        messageId.Add(new XAttribute(NameSpaces.xwsu + "Id", "messageID"));
        header.Add(messageId);
        messageId.Value = "urn:uuid:" + Guid.NewGuid().ToString("D");

        AddExtraHeaders(header);
        var security = AddWsSecurityHeader(BinarySecurityToken);
        AddWsuTimestamp(security);
    }

    public XDocument ToXml() => Build();

    public override string ToString() => ToXml().ToString();
}