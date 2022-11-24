using System.Security.Cryptography;
using Xunit;
using Shouldly;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json.Linq;
using static System.Convert;
using static System.Environment;
using static System.Text.Encoding;
using static System.Web.HttpUtility;
using Convert = System.Convert;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private static readonly X509Certificate2 Cert = LoadCertificate(StoreName.My, StoreLocation.CurrentUser);
        //X509Certificate2.CreateFromPemFile(certPemFilePath: "kit-test.cer", keyPemFilePath: "kit-test.pem");


    public static X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation storeLocation)
    {
        var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);
        const string cleanThumbprint = "3B0EC350637CB256D3DF4DBE45820C60311FA5AA";
        var result = store.Certificates.Find(X509FindType.FindByThumbprint, cleanThumbprint, false);

        if (result.Count == 0)
        {
            throw new ArgumentException("No certificate with thumbprint " + cleanThumbprint + " is found.");
        }

        return result[0];
    }

    [Fact]
    public async Task CallDemoServiceWithStsAssertion()
    {
        // Request for STS is created and signed.
        var stsRequest = new KombitStsRequest(
            endpointEntityId: "http://entityid.kombit.dk/service/demoservicerest/1",
            certificate: Cert,
            wsAddressingTo: new("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
            //municipalityCvr: 29189846); 
            municipalityCvr: 38163264);
        Should.NotThrow(() => VerifySignature(stsRequest.ToXDocument()).IfLeft(ex => throw ex));
        await File.WriteAllTextAsync("req.xml", stsRequest.ToString());

        // STS is called and the assertion in the response is extracted. The signature of the assertion is verified.
        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var httpMessageHandler = new HttpClientHandler();
        using var httpClient = new HttpClient(httpMessageHandler);
        var rawStsResponse = await (await httpClient.PostAsync(stsUri,
                new StringContent(stsRequest.ToString(), UTF8, "application/soap+xml"))).Content
            .ReadAsStringAsync();
        var stsResponse = XElement.Parse(rawStsResponse);
        var stsAssertion = stsResponse.Descendants(NameSpaces.xsaml + "Assertion")
            .HeadOrNone()
            .IfNone(() => throw Error.New($"STS error response:{NewLine}{stsResponse}"));
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
        httpClient.DefaultRequestHeaders.Add("x-TransaktionsId", Guid.NewGuid().ToString());
        httpClient.DefaultRequestHeaders.Add("x-TransaktionsTid", DateTime.UtcNow.ToString("u").Replace(" ", "T"));

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