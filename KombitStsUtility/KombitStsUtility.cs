using dk.nsi.seal.Model;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.DomBuilders;
using System.Xml.Linq;

namespace KombitStsUtilities;


public class KombitStsRequestBuilder : OioWsTrustRequestDomBuilder
{
    protected override void AddActAsTokens(XElement actAs)
    {
    }
}

public static class KombitSts
{
    public static string BuildRequest(Uri sts, string audience)
    {
        return new KombitStsRequestBuilder
        {
            Action = WsTrustConstants.Wst13IssueAction,
            WsAddressingTo = sts.ToString(),
            Audience = audience
        }
        .Build()
        .Document
        .ToString();
    }
}
