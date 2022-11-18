using Xunit;
using Shouldly;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml.Linq;
using LanguageExt;
using LanguageExt.Common;
using static System.Environment;

namespace KombitStsUtilityTests;

public class KombitStsUtilityTests
{
    private static readonly X509Certificate2 Cert =
        X509Certificate2.CreateFromPemFile(certPemFilePath: "kit-test.cer", keyPemFilePath: "kit-test.pem");

    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(
            endpointReference: "http://organisation.serviceplatformen.dk/service/organisation/5",
            certificate: Cert,
            wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
            municipalityCvr: 38163264);
        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());
        Should.NotThrow(() => VerifySignature(request.ToXml()).IfLeft(ex => throw ex));

        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var echoUri = new Uri("http://localhost:8686/RequestTest");
        var response = await KombitStsClient.GetAssertion(echoUri, request);

        // await File.WriteAllTextAsync("StsResponse.xml", response);

        // var encodedRequest = HttpUtility.UrlEncode(ToBase64String(UTF8.GetBytes(request.ToString()))); TODO

        // TODO Validate response from STS - use dk.nsi.seal.Model.OioWsTrustMessage and/or dk.nsi.seal.SealUtilities and/or dk.nsi.seal.SealSignedXml and/or dk.nsi.seal.Model.SignatureUtil as inspiration
        // var xmlSigner = new SignedXml(request.ToXml().ToXmlDocument());
        // xmlSigner.CheckSignature().ShouldBeTrue();
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

            var bVerifyRefDigests = false;
            var bSignatureVerified = dsig.VerifySignature(bVerifyRefDigests);
            if (!bSignatureVerified)
            {
                success = false;
                errorDescription.Append("Signature " + Convert.ToString(i + 1) + " invalid");
            }

            // Check each of the reference digests separately..
            var numRefDigests = dsig.NumReferences;
            var j = 0;
            while (j < numRefDigests)
            {
                var bDigestVerified = dsig.VerifyReferenceDigest(j);
                errorDescription.Append(NewLine + "reference digest " + Convert.ToString(j + 1) + " verified = " +
                                        Convert.ToString(bDigestVerified));
                if (bDigestVerified == false)
                {
                    success = false;
                    errorDescription.Append(NewLine + "    reference digest fail reason: " +
                                            Convert.ToString(dsig.RefFailReason));
                }

                j += 1;
            }

            i += 1;
        }

        return success
            ? Unit.Default
            : Error.New(errorDescription.ToString());
    }
}