using System.Net.Http.Headers;
using System.Web;
using System.Xml.Linq;
using static System.Convert;
using static System.Text.Encoding;

namespace KombitStsUtility
{
    public static class KombitStsClient
    {
        public static async Task<XDocument> GetAssertion(Uri stsUri, KombitStsRequest request)
        {
            throw new NotImplementedException();
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var response = await client.PostAsync(stsUri.ToString(), new StringContent(request.ToString()));

            return XDocument.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
