using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Model;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using SealTest.NSTWsProvider;
using static dk.nsi.seal.dgwstypes.SubjectIdentifierType;
using static dk.nsi.seal.MessageHeaders.XmlMessageHeader;
using static dk.nsi.seal.MessageHeaders.IdCardMessageHeader;

namespace SealTest.AssertionTests
{
    [TestFixture]
    public class WhiteSpaceInCardTest
    {
        private readonly X509Certificate2 userCert = new(
            $"{TestContext.CurrentContext.TestDirectory}/Resources/certificates/Sonja_Bach_Laege.p12", "Test1234");

        /// <summary>
        /// 1. Creating NemID as a Saml2Assertion
        /// 2. Exchanging the Saml2Assertion for a Sosi Card, created by the STS
        /// 3. Calling the National Test Service - NTS
        /// </summary>
        [Test]
        public void TestIDcard_Does_not_change_whiteSpace_Saml2SosiStsClient()
        {
            const string
            userCpr = "0309691444",
            userGivenName = "Sonja",
            userSurName = "Back",
            userEmail = "Sonja_Bach@nsi.dk",
            userRole = "7170",
            userAuthorizationCode = "NS363",
            cvr = "46837428";

            var systemInfo = new SystemInfo(new CareProvider(medcomcprnumber, cvr, "Statens Serum Institut"), "Sygdom.dk");
            var userInfo = new UserInfo(userCpr, userGivenName, userSurName, userEmail, "", userRole, userAuthorizationCode);

            var userIdCard = new UserIdCard(Configuration.SosiDgwsVersion,
                AuthenticationLevel.VocesTrustedSystem,
                "N/A",
                systemInfo,
                userInfo,
                null,
                new CertificateInfo(userCert).ToString(),
                null,
                null);

            var assertionBuilder = new OioSamlAssertionBuilder()
            {
                Issuer = "https://saml.test-nemlog-in.dk",
                UserIdCard = userIdCard,
                UserCertificate = userCert,
                SigningVault = new InMemoryCredentialVault(Global.StatensSerumInstitutFoces),
                NotBefore = DateTime.Now.AddDays(-1),
                NotOnOrAfter = DateTime.Now.AddHours(8),
                AudienceRestriction = "https://sts.sosi.dk/",
                RecipientUrl = "https://staging.fmk-online.dk/fmk/saml/SAMLAssertionConsumer"
            };

            var nemIdAssertion = assertionBuilder.Build();
            var idCard = ExchangeNemLoginAssertionForSosiSTSCard(userCert, nemIdAssertion, "NS363");

            CallNts(idCard);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(idCard.Xassertion));
        }

        /// <summary>
        /// 1. Creating a locally signed Seal Card
        /// 2. Getting STS to sign the card.
        /// 3. Calling the National Test Service - NTS
        /// </summary>
        [Test]
        public async Task TestIDcard_Does_not_change_whiteSpace()
        {
            var localIdc = new SystemIdCard { Xassertion = SerializerUtil.Serialize(AssertionMaker.MakeAssertionForSTS(Global.MocesCprGyldig, 4, "30808460")).Root };

            var sosiCardSTS = SealUtilities.SignIn(localIdc, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            await CallNts(sosiCardSTS);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(localIdc.Xassertion));
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(sosiCardSTS.Xassertion));
        }
         
        public static IdCard ExchangeNemLoginAssertionForSosiSTSCard(X509Certificate2 clientCertificate, OioSamlAssertion nemidAssertion, string authorizationCode) =>
            Saml2SosiStsClient.ExchangeAssertion(clientCertificate, new Uri(TestConstants.SecurityTokenServiceOioSaml2Sosi), nemidAssertion, authorizationCode);


        private static Task CallNts(IdCard idc)
        {
            var client = new NtsWSProviderClient(new BasicHttpBinding(), new EndpointAddress("http://test1.ekstern-test.nspop.dk:8080/nts/service"));
            client.Endpoint.EndpointBehaviors.Add(new SealEndpointBehavior());

            var dgwsHeader = new Header()
            {
                SecurityLevel = 4,
                SecurityLevelSpecified = true,
                Linking = new Linking { MessageID = Guid.NewGuid().ToString("D") }
            };

            using (new OperationContextScope(client.InnerChannel))
            {
                // Adding seal-security and dgws-header soap header
                OperationContext.Current.OutgoingMessageHeaders.Add(IdCardHeader(idc));
                OperationContext.Current.OutgoingMessageHeaders.Add(XmlHeader(dgwsHeader));

                // Throws Exception if not succesful.
                return client.invokeAsync("test");
            }
        }
    }
}