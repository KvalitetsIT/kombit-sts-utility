using Xunit;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private readonly static X509Certificate2 Cert = new("kit-test.pfx", "Test1234");

    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(endpoint: "http://organisation.serviceplatformen.dk/service/organisation/5",
                                           certificate: Cert,
                                           wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
                                           municipalityCvr: 38163264);

        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());

        var stsUri = new Uri("https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var response = await KombitStsClient.GetAssertion(stsUri, request);

        await File.WriteAllTextAsync("StsResponse.xml", response.ToString());

        // TODO Validate - use dk.nsi.seal.Model.OioWsTrustMessage and/or dk.nsi.seal.SealUtilities and/or dk.nsi.seal.SealSignedXml and/or dk.nsi.seal.Model.SignatureUtil as inspiration
    }
}