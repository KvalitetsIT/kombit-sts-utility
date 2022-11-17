using Xunit;
using Shouldly;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private readonly static X509Certificate2 Cert = X509Certificate2.CreateFromPemFile(certPemFilePath: "kit-test.cer", keyPemFilePath: "kit-test.pem");

    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(endpointReference: "http://organisation.serviceplatformen.dk/service/organisation/5",
                                           certificate: Cert,
                                           wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
                                           municipalityCvr: 38163264);

        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());

        var stsUri = new Uri("https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var echoUri = new Uri("http://localhost:8686/RequestTest");
        var response = await KombitStsClient.GetAssertion(stsUri, request);

        await File.WriteAllTextAsync("StsResponse.xml", response);

        // var encodedRequest = HttpUtility.UrlEncode(ToBase64String(UTF8.GetBytes(request.ToString()))); TODO

        // TODO Validate response from STS - use dk.nsi.seal.Model.OioWsTrustMessage and/or dk.nsi.seal.SealUtilities and/or dk.nsi.seal.SealSignedXml and/or dk.nsi.seal.Model.SignatureUtil as inspiration
        // var xmlSigner = new SignedXml(request.ToXml().ToXmlDocument());
        // xmlSigner.CheckSignature().ShouldBeTrue();
    }
}