using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using NUnit.Framework;
using dk.nsi.seal.Model;
using System;
using dk.nsi.seal.Vault;
using System.ServiceModel;
using SealTest.NSTWsProvider;
using System.Threading.Tasks;
using dk.nsi.seal.dgwstypes;
using static dk.nsi.seal.dgwstypes.SubjectIdentifierType;
using static dk.nsi.seal.MessageHeaders.XmlMessageHeader;
using static dk.nsi.seal.MessageHeaders.IdCardMessageHeader;

namespace SealTest.AssertionTests
{
    [TestFixture]
    public class NemIdAssertionTest
    {
        /**
         * In this example the user has a single authorization in the authorization register. The id card request is
         * made for that one specific authorization.
         */

        [Test]
        public async Task UserWithOneSpecificAuthorization()
        {
            const string keystorePath = "Karl_Hoffmann_Svendsen_Laege.p12";
            const string userCpr = "0102732379";
            const string userGivenName = "Karl Hoffmann";
            const string userSurName = "Svendsen";
            const string userEmail = "Karl_Hoffmann_Svendsen@nsi.dk";
            const string userRole = "7170";
            const string userAuthorizationCode = "NS362";

            var idCard = await TestNemId2SealAssertion(keystorePath, userCpr, userGivenName, userSurName, userEmail, userRole, userAuthorizationCode);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(idCard.Xassertion));
            Assert.IsNotNull(idCard.IdCardId, "No user information found");
            //assertEquals("Incorrect authorization code", "NS362", idCard.getUserInfo().getAuthorizationCode());
            //assertEquals("Incorrect education code", "7170", idCard.getUserInfo().getRole());
        }

        /**
         * In this example the user has a single authorization in the authorization register. The id card request is
         * made with only a limitations on the role/education code. This is only possible because the user has one
         * and only one authorization with the "Doctor" role.
         */

        [Test]
        public async Task UserWithOneDoctorAuthorization()
        {
            const string keystorePath = "Karl_Hoffmann_Svendsen_Laege.p12";
            const string userCpr = "0102732379";
            const string userGivenName = "Karl Hoffmann";
            const string userSurName = "Svendsen";
            const string userEmail = "Karl_Hoffmann_Svendsen@nsi.dk";
            const string userRole = "7170";
            const string userAuthorizationCode = null;

            var idCard = await TestNemId2SealAssertion(keystorePath, userCpr, userGivenName, userSurName, userEmail, userRole, userAuthorizationCode);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(idCard.Xassertion));
            //assertNotNull("No user information found", idCard.getUserInfo());
            //assertEquals("Incorrect authorization code", "NS362", idCard.getUserInfo().getAuthorizationCode());
            //assertEquals("Incorrect education code", "7170", idCard.getUserInfo().getRole());
        }

        /**
         * In this example the user has a single authorization in the authorization register. The id card request is
         * made without a limitations on the authorization that will be used. This is only possible because the user
         * has one and only one authorization.
         */

        [Test]
        public async Task UserWithOneAuthorization()
        {
            const string keystorePath = "Karl_Hoffmann_Svendsen_Laege.p12";
            const string userCpr = "0102732379";
            const string userGivenName = "Karl Hoffmann";
            const string userSurName = "Svendsen";
            const string userEmail = "Karl_Hoffmann_Svendsen@nsi.dk";
            const string userRole = "IGNORED"; // Must not be an empty string or null, but all values that are not four digits are ignored by STS
            const string userAuthorizationCode = null;

            var idCard = await TestNemId2SealAssertion(keystorePath, userCpr, userGivenName, userSurName, userEmail, userRole, userAuthorizationCode);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(idCard.Xassertion));
            //assertNotNull("No user information found", idCard.getUserInfo());
            //assertEquals("Incorrect authorization code", "NS362", idCard.getUserInfo().getAuthorizationCode());
            //assertEquals("Incorrect education code", "7170", idCard.getUserInfo().getRole());
        }


        /**
         * In this example the user has two authorizations in the authorization register. The id card request is
         * made for one specifik authorization. If no limitations where made in the request, the call would fail
         * because STS will not choose one at random for us.
         */

        [Test]
        public async Task UserWithSeveralAuthorization()
        {
            const string keystorePath = "Sonja_Bach_Laege.p12";
            const string userCpr = "0309691444";
            const string userGivenName = "Sonja";
            const string userSurName = "Bach";
            const string userEmail = "Sonja_Bach@nsi.dk";
            const string userRole = "7170";
            const string userAuthorizationCode = "NS363";

            var idCard = await TestNemId2SealAssertion(keystorePath, userCpr, userGivenName, userSurName, userEmail, userRole, userAuthorizationCode);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(idCard.Xassertion));
            //assertNotNull("No user information found", idCard.getUserInfo());
            //assertEquals("Incorrect authorization code", "NS363", idCard.getUserInfo().getAuthorizationCode());
            //assertEquals("Incorrect education code", "7170", idCard.getUserInfo().getRole());
        }

        public static async Task<IdCard> TestNemId2SealAssertion(string keystorePath,
            string userCpr,
            string userGivenName,
            string userSurName,
            string userEmail,
            string userRole,
            string userAuthorizationCode)
        {
            const string cvr = "46837428";
            var statensSerumInstitutFoces = Global.StatensSerumInstitutFoces;
            var userCert = new X509Certificate2(TestContext.CurrentContext.TestDirectory + "/Resources/certificates/" + keystorePath, "Test1234");
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

            var nemIdAssertion = new OioSamlAssertionBuilder()
            {
                Issuer = "https://saml.test-nemlog-in.dk",
                UserIdCard = userIdCard,
                UserCertificate = userCert,
                SigningVault = new InMemoryCredentialVault(statensSerumInstitutFoces),
                NotBefore = DateTime.Now.AddDays(-1),
                NotOnOrAfter = DateTime.Now.AddHours(8),
                AudienceRestriction = "https://sts.sosi.dk/",
                RecipientUrl = "https://staging.fmk-online.dk/fmk/saml/SAMLAssertionConsumer"
            }
            .Build();

            var idCard = ExchangeNemLoginAssertionForSosiSTSCard(statensSerumInstitutFoces, nemIdAssertion, userAuthorizationCode);

            await CallNts(idCard);

            return idCard;
        }

        private static Task CallNts(IdCard idc)
        {
            var client = new NtsWSProviderClient(new BasicHttpsBinding(), new EndpointAddress("https://test1-cnsp.ekstern-test.nspop.dk:8443/nts/service"));
            client.Endpoint.EndpointBehaviors.Add(new SealEndpointBehavior());

            var dgwsHeader = new Header 
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

        public static IdCard ExchangeNemLoginAssertionForSosiSTSCard(X509Certificate2 clientCertificate, OioSamlAssertion nemidAssertion, string authorizationCode) =>
            Saml2SosiStsClient.ExchangeAssertion(clientCertificate, new Uri(TestConstants.SecurityTokenServiceOioSaml2Sosi), nemidAssertion, authorizationCode);
    }
}