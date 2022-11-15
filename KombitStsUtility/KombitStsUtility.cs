using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.DomBuilders;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.Model;

namespace KombitStsUtility;

public class KombitStsRequest : OioWsTrustDomBuilder
{
    public string WsAddressingTo { get; init; } = "";

    public new string Action { get; init; } = "";

    public string Audience { get; }

    public string BinarySecurityToken { get; }

    private readonly static XAttribute ValueType = new("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");

    private readonly static XAttribute EncodingType = new("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

    public KombitStsRequest(string audience, string binarySecurityToken)
    {
        base.Action = Action;
        this.Audience = audience;
        this.BinarySecurityToken = binarySecurityToken;
    }

    protected override void AddBodyContent(XElement body)
    {
        body.Add(RequestSecurityToken(Audience, BinarySecurityToken));
    }


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

    protected override void AddExtraHeaders(XElement header)
    {
        var to = new XElement(NameSpaces.xwsa + "To");
        header.Add(to);
        to.Value = WsAddressingTo;
    }

    protected override void AddExtraNamespaceDeclarations(XElement envelope)
    {
    }

    protected override void ValidateBeforeBuild()
    {
    }

    public XDocument ToXml() => Build();

    public override string ToString() => ToXml().ToString();
}