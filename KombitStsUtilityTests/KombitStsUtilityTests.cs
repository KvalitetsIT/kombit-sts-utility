using Xunit;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private static X509Certificate2 SelfSignedCert => new CertificateRequest("cn=foobar", RSA.Create(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                                                            .CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(endpoint: "http://organisation.serviceplatformen.dk/service/organisation/5",
                                           certificate: SelfSignedCert,
                                           wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
                                           municipalityCvr: 38163264);

        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());

        var response = KombitStsClient.GetAssertion(request);

        // TODO Validate - use dk.nsi.seal.Model.OioWsTrustMessage and/or dk.nsi.seal.SealUtilities and/or dk.nsi.seal.SealSignedXml and/or dk.nsi.seal.Model.SignatureUtil as inspiration
    }
}