using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using LanguageExt;
using static System.Text.Encoding;

namespace KombitStsUtility;

public static class KombitStsClient
{
    public static Task<HttpResponseMessage> GetAssertion(Uri stsUri, KombitStsRequest request)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(request.Certificate);
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("soapaction", WsTrustConstants.Wst13IssueAction);
        return client.PostAsync(stsUri.ToString(), new StringContent(request.ToString(), UTF8, "application/soap+xml"));
    }

    public static async Task<string> GetAssertion(Uri stsUri, string request, X509Certificate cert) // TODO
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("soapaction", WsTrustConstants.Wst13IssueAction);
        var response =
            await client.PostAsync(stsUri.ToString(), new StringContent(request, UTF8, "application/soap+xml"));
        return await response.Content.ReadAsStringAsync();
    }
}