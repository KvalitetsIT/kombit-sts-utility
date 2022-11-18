using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using static System.Text.Encoding;

namespace KombitStsUtility;

public static class KombitStsClient
{
    public static async Task<XDocument> GetAssertion(Uri stsUri, KombitStsRequest request)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(request.Certificate);
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("soapaction", WsTrustConstants.Wst13IssueAction);
        var response = await client.PostAsync(stsUri.ToString(), new StringContent(request.ToString(), UTF8, "application/soap+xml"));
        return XDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}