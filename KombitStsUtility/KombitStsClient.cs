using System.Net.Http.Headers;
using System.Web;
using System.Xml.Linq;
using static System.Convert;
using static System.Text.Encoding;

namespace KombitStsUtility
{
    public static class KombitStsClient
    {
        public static async Task<string> GetAssertion(Uri stsUri, KombitStsRequest request)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("soapaction", "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue");

            var response = await client.PostAsync(stsUri.ToString(), new StringContent(request.ToString(), UTF8, "application/soap+xml"));

            return await response.Content.ReadAsStringAsync();
        }
    }
}