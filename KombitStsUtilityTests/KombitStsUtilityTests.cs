using Xunit;
using Shouldly;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using dk.nsi.seal;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnitTesting;
using Newtonsoft.Json.Linq;
using static System.Convert;
using static System.Environment;
using static System.Text.Encoding;
using static System.Web.HttpUtility;
using Convert = System.Convert;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private static readonly X509Certificate2 Cert =
        X509Certificate2.CreateFromPemFile(certPemFilePath: "kit-test.cer", keyPemFilePath: "kit-test.pem");

    [Fact]
    public async Task GetRequestFromStsShouldReturnAssertion()
    {
        // Request for STS is created and signed.
        var stsRequest = new KombitStsRequest(
            endpointEntityId: "http://entityid.kombit.dk/service/demoservicerest/1",
            certificate: Cert,
            wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
            municipalityCvr: 38163264);
        Should.NotThrow(() => VerifySignature(stsRequest.ToXDocument()).IfLeft(ex => throw ex));

        // STS is called and the assertion in the response is extracted. The signature of the assertion is verified.
        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var httpMessageHandler = new HttpClientHandler();
        using var httpClient = new HttpClient(httpMessageHandler);
        var stsResponse = await (await httpClient.PostAsync(stsUri.ToString(),
                new StringContent(stsRequest.ToString(), UTF8, "application/soap+xml")))
            .Content
            .ReadAsStringAsync();
        var stsAssertion = XElement.Parse(stsResponse).Descendants(NameSpaces.xsaml + "Assertion").First();
        Should.NotThrow(() => VerifySignature(stsAssertion).IfLeft(ex => throw ex));

        // The STS assertion is encoded and send to the token endpoint of the service we would like to communicate with.
        // The access token in the response is extracted.
        var encodedStsAssertion = UrlEncode(
            ToBase64String(UTF8.GetBytes(stsAssertion.ToString(SaveOptions.DisableFormatting))));
        var accessTokenUri = new Uri("https://exttest.serviceplatformen.dk/service/AccessTokenService_1/token");
        httpMessageHandler.ClientCertificates.Add(Cert);
        var httpResponseMessage = (await httpClient.PostAsync(accessTokenUri,
            new StringContent("saml-token=" + encodedStsAssertion, UTF8, "application/x-www-form-urlencoded")));
        var accessToken = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync())
            .Value<string>("access_token");

        // OIO IDWS REST demo service is called using the access token
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Holder-of-key {accessToken}");
        httpClient.DefaultRequestHeaders.Add("x-TransaktionsId", "123456");
        httpClient.DefaultRequestHeaders.Add("x-TransaktionsTid", "2021-07-09T10:15:30+01:00");
        var serviceUri =
            new Uri(
                "https://exttest.serviceplatformen.dk/service/AccessTokenDemo_1/callDemoService/Testing_1_2_3_Testing");
        var serviceResponse = await (await httpClient.GetAsync(serviceUri))
            .Content.ReadAsStringAsync();

        JObject.Parse(serviceResponse).Value<string>("data").ShouldBe("OK");
    }

    private static Either<Error, Unit> VerifySignature(XNode doc)
    {
        var dsig = new Chilkat.XmlDSig();
        dsig.LoadSignature(doc.ToString(SaveOptions.DisableFormatting));

        var errorDescription = new StringBuilder();

        var numSignatures = dsig.NumSignatures;
        var i = 0;
        var success = true;
        while (i < numSignatures)
        {
            dsig.Selector = i;

            const bool verifyRefDigests = false;
            var signatureVerified = dsig.VerifySignature(verifyRefDigests);
            if (signatureVerified)
            {
                errorDescription.Append("Signature " + Convert.ToString(i + 1) + " verified");
            }
            else
            {
                success = false;
                errorDescription.Append("Signature " + Convert.ToString(i + 1) + " invalid");
            }

            // Check each of the reference digests separately..
            var numRefDigests = dsig.NumReferences;
            var j = 0;
            while (j < numRefDigests)
            {
                var digestVerified = dsig.VerifyReferenceDigest(j);
                errorDescription.Append(NewLine + "reference digest " + Convert.ToString(j + 1) + " verified = " +
                                        Convert.ToString(digestVerified));
                if (digestVerified == false)
                {
                    success = false;
                    errorDescription.Append(NewLine + "    reference digest fail reason: " +
                                            Convert.ToString(dsig.RefFailReason));
                }

                j += 1;
            }

            errorDescription.Append(NewLine + NewLine);

            i += 1;
        }

        return success
            ? Unit.Default
            : Error.New(errorDescription.ToString());
    }
}