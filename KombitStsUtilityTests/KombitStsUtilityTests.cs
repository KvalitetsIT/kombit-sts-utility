using Xunit;
using Shouldly;
using KombitStsUtility;
using System.Security.Cryptography.X509Certificates;
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
        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToPrettyString());
        Should.NotThrow(() => VerifySignature(request.ToDom()).IfLeft(ex => throw ex));
        
        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var response = await KombitStsClient.GetAssertion(stsUri, request);
        await File.WriteAllTextAsync("StsResponse.xml", (await response.Content.ReadAsStringAsync()));

        // var encodedRequest = HttpUtility.UrlEncode(ToBase64String(UTF8.GetBytes(request.ToString()))); TODO
        // TODO Validate response from STS - use dk.nsi.seal.Model.OioWsTrustMessage and/or dk.nsi.seal.SealUtilities and/or dk.nsi.seal.SealSignedXml and/or dk.nsi.seal.Model.SignatureUtil as inspiration
        // var xmlSigner = new SignedXml(request.ToXml().ToXmlDocument());
        // var res = dsig.VerifySignature(true).ShouldBeTrue();
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
            if (!signatureVerified)
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

            i += 1;
        }

        return success
            ? Unit.Default
            : Error.New(errorDescription.ToString());
    }
}