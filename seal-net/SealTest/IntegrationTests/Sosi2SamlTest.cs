using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using NUnit.Framework;
using SealTest.SosiGW;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GW = SealTest.SosiGW;
using static SealTest.SosiGW.SosiGWFacadeClient.EndpointConfiguration;
using SubjectIdentifierType = dk.nsi.seal.dgwstypes.SubjectIdentifierType;
using SealTest.Certificate;

namespace SealTest.AssertionTests
{
    public class Sosi2SamlTest
    {
        

        private static X509Certificate2 UserCert => new CertificateOces2MocesCpr().Certificate;

        public static UserIdCard IdCard
        {
            get
            {
                const string
                cvr = "30808460",
                orgName = "Test",
                itSystemName = "Test",
                givenName = "NA",
                surName = "NA",
                email = "NA",
                occupation = null,
                role = "NA",
                authorizationcode = null,
                userName = "",
                password = "",
                cpr = "1802602810";

                return new UserIdCard(Configuration.SosiDgwsVersion,
                    AuthenticationLevel.MocesTrustedUser,
                    "https://saml.test-nemlog-in.dk/",
                    new SystemInfo(new CareProvider(SubjectIdentifierType.medcomcvrnumber, cvr, orgName), itSystemName),
                    new UserInfo(cpr, givenName, surName, email, occupation, role, authorizationcode),
                    null,
                    "nameId",
                    userName,
                    password
                    );
            }
        }

        public static async Task LoginToGateway(IdCard idCard, X509Certificate2 userCert)
        {
            var sosiGwAssertion = idCard.GetAssertion<AssertionType>();
            var security = new GW.Security
            {
                Timestamp = new GW.Timestamp { Created = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5) },
                Assertion = sosiGwAssertion
            };
            const string endpointAddress = "http://test1.ekstern-test.nspop.dk:8080/sosigw/service/sosigw";
            using var gwClient = new SosiGWFacadeClient(SosiGWSoapBinding, endpointAddress);
            var dig = (await gwClient.requestIdCardDigestForSigningAsync(security, "whatever")).requestIdCardDigestForSigningResponse;
            var digestHash = SHA1.HashData(dig.DigestValue);
            var signature = userCert.GetRSAPrivateKey().SignHash(digestHash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            var cardRequestBody = new signIdCardRequestBody
            {
                SignatureValue = signature,
                KeyInfo = new GW.KeyInfo
                {
                    Item = new GW.X509Data { Item = userCert.Export(X509ContentType.Cert) }
                }
            };
            // The STS signed card is saved in the sosiGw cache and is used for future calls through the proxy
            var res = (await gwClient.signIdCardAsync(security, cardRequestBody)).signIdCardResponse;
            if (res != signIdCardResponse.ok) { throw new Exception("Gateway logon error"); }
            //Convert the GW Assertion to a dgwsType Assertion
            var sosiGwAssertionDocument = SerializerUtil.Serialize(sosiGwAssertion).Root;
            SerializerUtil.Deserialize<Assertion>(sosiGwAssertionDocument, typeof(AssertionType).Name);
        }

        [Test]
        public async Task GivenClientThenExchangeAssertionShouldReturnToken()
        {
            await LoginToGateway(IdCard, UserCert);
            var uri = new Uri("http://test1.ekstern-test.nspop.dk:8080/sosigw/proxy/soap-request");
            var response = Sosi2SamlStsClient.ExchangeAssertion(uri, "/ststest", IdCard);
            Assert.IsFalse(response.IsFault);
        }

        [Test]
        public async Task GivenClientThenExchangeAssertionWithInvalidAudienceShouldReturnFault()
        {
            await LoginToGateway(IdCard, UserCert);
            var uri = new Uri("http://test1.ekstern-test.nspop.dk:8080/sosigw/proxy/soap-request");
            var response = Sosi2SamlStsClient.ExchangeAssertion(uri, "Invalid audience", IdCard);
            Assert.IsTrue(response.IsFault);
        }
    }
}