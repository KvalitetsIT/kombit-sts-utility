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

    private const string req = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Header><Action xmlns=\"http://www.w3.org/2005/08/addressing\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"_7ef2e6f4-765c-4326-8448-6b60166a7955\">http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</Action><MessageID xmlns=\"http://www.w3.org/2005/08/addressing\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"_30b58956-465c-4289-bcfb-cbf9eff3a365\">urn:uuid:248332d4-7375-4e36-818b-1f5345c41bdd</MessageID><To xmlns=\"http://www.w3.org/2005/08/addressing\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"_e49b61fc-7376-44ae-905d-fb64b73f129b\">https://echo:8443/runtime/services/kombittrust/14/certificatemixed</To><ReplyTo xmlns=\"http://www.w3.org/2005/08/addressing\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"_be574603-3725-49cb-bc08-3d61a46d25a4\"><Address>http://www.w3.org/2005/08/addressing/anonymous</Address></ReplyTo><wsse:Security xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" soap:mustUnderstand=\"1\"><wsse:BinarySecurityToken xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\" ValueType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3\" wsu:Id=\"X509-c5bd704d-309e-4f19-aff8-13a0821ad62f\">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</wsse:BinarySecurityToken><wsu:Timestamp xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"TS-433efc59-712b-423a-a3e3-ad148992e263\"><wsu:Created>2022-11-22T07:33:00.000Z</wsu:Created><wsu:Expires>2022-11-22T07:38:00.000Z</wsu:Expires></wsu:Timestamp></wsse:Security></soap:Header><soap:Body xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" wsu:Id=\"_149c253c-96ed-419f-af8d-fd5dfbe8d874\"><wst:RequestSecurityToken xmlns:wst=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512\"><wst:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0</wst:TokenType><wst:RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</wst:RequestType><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><adr:EndpointReference xmlns:adr=\"http://www.w3.org/2005/08/addressing\"><adr:Address>http://organisation.serviceplatformen.dk/service/organisation/5</adr:Address></adr:EndpointReference></wsp:AppliesTo><wst:Claims Dialect=\"http://docs.oasis-open.org/wsfed/authorization/200706/authclaims\"><wsfed:ClaimType xmlns:wsfed=\"http://docs.oasis-open.org/wsfed/authorization/200706\" Uri=\"dk:gov:saml:attribute:CvrNumberIdentifier\"><wsfed:Value>38163264</wsfed:Value></wsfed:ClaimType></wst:Claims><wst:KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey</wst:KeyType><wst:UseKey><wsse:BinarySecurityToken xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\" ValueType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3\">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</wsse:BinarySecurityToken></wst:UseKey></wst:RequestSecurityToken></soap:Body></soap:Envelope>";

    // actionRef = "_" + "7ef2e6f4-765c-4326-8448-6b60166a7955",
    // messageIdRef = "_" + "30b58956-465c-4289-bcfb-cbf9eff3a365",
    // toRef = "_" + "e49b61fc-7376-44ae-905d-fb64b73f129b",
    // replyToRef = "_" + "be574603-3725-49cb-bc08-3d61a46d25a4",
    // timestampRef = "TS-" + "433efc59-712b-423a-a3e3-ad148992e263",
    // binarySecurityTokenRef = "X509-" + "c5bd704d-309e-4f19-aff8-13a0821ad62f",
    // bodyRef = "_" + "149c253c-96ed-419f-af8d-fd5dfbe8d874";
    
    [Fact]
    public async Task T() // TODO
    {
        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        var response = await KombitStsClient.GetAssertion(stsUri,
            KombitStsRequest.XmlSigner.Sign(Cert, XDocument.Parse(req)).ToString(SaveOptions.DisableFormatting), Cert);
        await File.WriteAllTextAsync("StsResponse.xml", response);
    }

    [Fact]
    public async Task WriteRequestToFile() // TODO
    {
        var request = new KombitStsRequest(
            endpointReference: "http://organisation.serviceplatformen.dk/service/organisation/5",
            certificate: Cert,
            wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
            municipalityCvr: 38163264);
        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());
        Should.NotThrow(() => VerifySignature(request.ToDom()).IfLeft(ex => throw ex));
    }

    [Fact]
    public async Task RequestShouldBeCorrect()
    {
        var request = new KombitStsRequest(
            endpointReference: "http://organisation.serviceplatformen.dk/service/organisation/5",
            certificate: Cert,
            wsAddressingTo: new Uri("https://echo:8443/runtime/services/kombittrust/14/certificatemixed"),
            municipalityCvr: 38163264);
        await File.WriteAllTextAsync("GeneratedRequest.xml", request.ToString());
        Should.NotThrow(() => VerifySignature(request.ToDom()).IfLeft(ex => throw ex));

        var stsUri =
            new Uri(
                "https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed");
        
        var response = await KombitStsClient.GetAssertion(stsUri, request);
        await File.WriteAllTextAsync("StsResponse.xml", await response.Content.ReadAsStringAsync());

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