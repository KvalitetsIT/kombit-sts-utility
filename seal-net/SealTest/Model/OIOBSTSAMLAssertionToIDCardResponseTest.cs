using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Federation;
using dk.nsi.seal.Model.ModelBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using System.Xml.Linq;

namespace SealTest.Model
{
    class OIOBSTSAMLAssertionToIDCardResponseTest
    {
        public ICredentialVault responseSigningVault;

        [SetUp]
        public void Setup() => responseSigningVault = CredentialVaultTestUtil.GetCredentialVaultFromResource("teststs-1.p12");

        [Test]
        public void TestSAMLAssertionToIDCardResponseModelBuilderOIOH2BST()
        {
            // GIVEN
            OIOBSTSAMLAssertionToIDCardResponseModelBuilder modelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder();
            XDocument build = XDocument.Load(TestContext.CurrentContext.TestDirectory + "/Resources/oiosaml-examples/OIOSAML3Assertion/OIOH2BST2Sosi-response.xml");

            // WHEN
            var response = modelBuilder.Build(build);

            // THEN
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(new SosiTestFederation(new CrlCertificateStatusChecker()));
            Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", response.Action, "Uventet action");
            Assert.AreEqual("https://fmk", response.AppliesTo, "Uventet Applies To/Audience Restriction");
            Assert.AreEqual("https://fmk", response.Context, "Uventet Context");
            Assert.NotNull(response.Created, "Forventede Created Date i response");
            Assert.NotNull(response.Expires, "Forventede Expireds Date i response");
            Assert.False(response.IsFault, "Forventede ikke fejl");
            Assert.Null(response.FaultActor, "Forventede ikke fejl info");
            Assert.Null(response.FaultCode, "Forventede ikke fejl info");
            Assert.Null(response.FaultString, "Forventede ikke fejl info");
            Assert.NotNull(response.MessageID, "Forventede Message uuid i reponse");
            Assert.AreEqual("urn:uuid:887eef4b-d2cc-427d-b189-e76c9ed7ea27", response.RelatesTo, "Uventet Relates To");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), response.GetSigningCertificate(), "Uventet Signing Certificate");
            Assert.AreEqual("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0", response.TokenType, "Uventet Token Type");

            Assert.True(response.IdCard is UserIdCard, "Forventede UserIDCard");
            UserIdCard idCard = (UserIdCard)response.IdCard;

            Assert.AreEqual("Mads_Skjern", idCard.AlternativeIdentifier, "Uventet Alternativ Identifier/Subject Name ID");
            Assert.AreEqual(4, idCard.AuthenticationLevel.Level, "Uventet AuthenticationLevel");
            Assert.Null(idCard.CertHash, "Forventede ikke cert hash i ID Card");
            Assert.NotNull(idCard.CreatedDate, "Forventede Created Date i ID Card");
            Assert.NotNull(idCard.ExpiryDate, "Forventede Expiry Date i ID Card");
            Assert.NotNull(idCard.IdCardId, "Forventede ID Card id i ID Card");
            Assert.AreEqual(idCard.Issuer, "TEST1-NSP-STS", "TheSOSILibrary");
            Assert.Null(idCard.Password, "Forventede ikke password i ID Card");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), idCard.SignedByCertificate, "Uventet Signing certificate");
            Assert.Null(idCard.Username, "Forventede ikke username i ID Card");
            Assert.AreEqual("1.0.1", idCard.Version, "Uventet version");

            SystemInfo systemInfo = idCard.SystemInfo;
            Assert.AreEqual("Korsbæk Kommunes IT systemer", systemInfo.ItSystemName, "Uventet IT System Name");
            Assert.AreEqual("20301823", systemInfo.CareProvider.Id, "Uventet Care Provider Id");
            Assert.AreEqual("Korsbæk Kommune", systemInfo.CareProvider.OrgName, "Uventet Care Provider Organization Name");
            Assert.AreEqual(SubjectIdentifierType.medcomcvrnumber, systemInfo.CareProvider.Type, "Uventet Care Provider Id Type");

            UserInfo userInfo2 = idCard.UserInfo;
            Assert.Null(userInfo2.AuthorizationCode, "Uventet Authorization Code");
            Assert.AreEqual("1212714321", userInfo2.Cpr, "Uventet CPR");
            Assert.Null(userInfo2.Email, "Forventede ikke Email i ID card");
            Assert.AreEqual("NSTSSnullAnull", userInfo2.GivenName, "Uventet Given Name");
            Assert.Null(userInfo2.Occupation, "Forventede ikke Occupation i ID card");
            Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:99991:value:TestRolle1", userInfo2.Role, "Uventet Role");
            Assert.AreEqual("Olsen", userInfo2.SurName, "Uventet Sur Name");

            Assert.False(idCard.IsValidInTime, "Forventede kke ID card var valid");

            idCard.ValidateSignature();
            idCard.ValidateSignatureAndTrust();
        }

        [Test]
        public void TestSAMLAssertionToIDCardResponseModelBuilderOIOH3BST()
        {
            // GIVEN
            OIOBSTSAMLAssertionToIDCardResponseModelBuilder modelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder();
            XDocument build = XDocument.Load(TestContext.CurrentContext.TestDirectory + "/Resources/oiosaml-examples/OIOSAML3Assertion/OIOH3BST2Sosi-response.xml");

            // WHEN
            var response = modelBuilder.Build(build);

            // THEN
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(new SosiTestFederation(new CrlCertificateStatusChecker()));
            Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", response.Action, "Uventet action");
            Assert.AreEqual("https://fmk", response.AppliesTo, "Uventet Applies To/Audience Restriction");
            Assert.AreEqual("https://fmk", response.Context, "Uventet Context");
            Assert.NotNull(response.Created, "Forventede Created Date i response");
            Assert.NotNull(response.Expires, "Forventede Expireds Date i response");
            Assert.False(response.IsFault, "Forventede ikke fejl");
            Assert.Null(response.FaultActor, "Forventede ikke fejl info");
            Assert.Null(response.FaultCode, "Forventede ikke fejl info");
            Assert.Null(response.FaultString, "Forventede ikke fejl info");
            Assert.NotNull(response.MessageID, "Forventede Message uuid i reponse");
            Assert.AreEqual("urn:uuid:bdf388cb-7145-441b-a8b7-1cc3886d1690", response.RelatesTo, "Uventet Relates To");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), response.GetSigningCertificate(), "Uventet Signing Certificate");
            Assert.AreEqual("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0", response.TokenType, "Uventet Token Type");

            Assert.True(response.IdCard is UserIdCard, "Forventede UserIDCard");
            UserIdCard idCard = (UserIdCard)response.IdCard;

            Assert.AreEqual("Mads_Skjern", idCard.AlternativeIdentifier, "Uventet Alternativ Identifier/Subject Name ID");
            Assert.AreEqual(4, idCard.AuthenticationLevel.Level, "Uventet AuthenticationLevel");
            Assert.Null(idCard.CertHash, "Forventede ikke cert hash i ID Card");
            Assert.NotNull(idCard.CreatedDate, "Forventede Created Date i ID Card");
            Assert.NotNull(idCard.ExpiryDate, "Forventede Expiry Date i ID Card");
            Assert.NotNull(idCard.IdCardId, "Forventede ID Card id i ID Card");
            Assert.AreEqual(idCard.Issuer, "TEST1-NSP-STS", "TheSOSILibrary");
            Assert.Null(idCard.Password, "Forventede ikke password i ID Card");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), idCard.SignedByCertificate, "Uventet Signing certificate");
            Assert.Null(idCard.Username, "Forventede ikke username i ID Card");
            Assert.AreEqual("1.0.1", idCard.Version, "Uventet version");

            SystemInfo systemInfo = idCard.SystemInfo;
            Assert.AreEqual("Korsbæk Kommunes IT systemer", systemInfo.ItSystemName, "Uventet IT System Name");
            Assert.AreEqual("20301823", systemInfo.CareProvider.Id, "Uventet Care Provider Id");
            Assert.AreEqual("Korsbæk Kommune", systemInfo.CareProvider.OrgName, "Uventet Care Provider Organization Name");
            Assert.AreEqual(SubjectIdentifierType.medcomcvrnumber, systemInfo.CareProvider.Type, "Uventet Care Provider Id Type");

            UserInfo userInfo2 = idCard.UserInfo;
            Assert.AreEqual("005NX", userInfo2.AuthorizationCode, "Uventet Authorization Code");
            Assert.AreEqual("0808754321", userInfo2.Cpr, "Uventet CPR");
            Assert.Null(userInfo2.Email, "Forventede ikke Email i ID card");
            Assert.AreEqual("NSTSSnullAtre", userInfo2.GivenName, "Uventet Given Name");
            Assert.Null(userInfo2.Occupation, "Forventede ikke Occupation i ID card");
            Assert.AreEqual("7170", userInfo2.Role, "Uventet Role");
            Assert.AreEqual("Larsen", userInfo2.SurName, "Uventet Sur Name");

            Assert.False(idCard.IsValidInTime, "Forventede ikke ID card var valid");

            idCard.ValidateSignature();
            idCard.ValidateSignatureAndTrust();
        }

        [Test]
        public void TestSAMLAssertionToIDCardResponseModelBuilderOIO3BST()
        {
            // GIVEN
            OIOBSTSAMLAssertionToIDCardResponseModelBuilder modelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder();
            XDocument build = XDocument.Load(TestContext.CurrentContext.TestDirectory + "/Resources/oiosaml-examples/OIOSAML3Assertion/OIO3BST2Sosi-response.xml");

            // WHEN
            var response = modelBuilder.Build(build);

            // THEN
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(new SosiTestFederation(new CrlCertificateStatusChecker()));
            Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", response.Action, "Uventet action");
            Assert.AreEqual("https://fmk", response.AppliesTo, "Uventet Applies To/Audience Restriction");
            Assert.AreEqual("https://fmk", response.Context, "Uventet Context");
            Assert.NotNull(response.Created, "Forventede Created Date i response");
            Assert.NotNull(response.Expires, "Forventede Expireds Date i response");
            Assert.False(response.IsFault, "Forventede ikke fejl");
            Assert.Null(response.FaultActor, "Forventede ikke fejl info");
            Assert.Null(response.FaultCode, "Forventede ikke fejl info");
            Assert.Null(response.FaultString, "Forventede ikke fejl info");
            Assert.NotNull(response.MessageID, "Forventede Message uuid i reponse");
            Assert.AreEqual("urn:uuid:11249c01-5b74-45ab-89e3-bf51e1b06ef5", response.RelatesTo, "Uventet Relates To");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), response.GetSigningCertificate(), "Uventet Signing Certificate");
            Assert.AreEqual("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0", response.TokenType, "Uventet Token Type");

            Assert.True(response.IdCard is UserIdCard, "Forventede UserIDCard");
            UserIdCard idCard = (UserIdCard)response.IdCard;

            Assert.AreEqual("Mads_Skjern", idCard.AlternativeIdentifier, "Uventet Alternativ Identifier/Subject Name ID");
            Assert.AreEqual(4, idCard.AuthenticationLevel.Level, "Uventet AuthenticationLevel");
            Assert.Null(idCard.CertHash, "Forventede ikke cert hash i ID Card");
            Assert.NotNull(idCard.CreatedDate, "Forventede Created Date i ID Card");
            Assert.NotNull(idCard.ExpiryDate, "Forventede Expiry Date i ID Card");
            Assert.NotNull(idCard.IdCardId, "Forventede ID Card id i ID Card");
            Assert.AreEqual(idCard.Issuer, "TEST1-NSP-STS", "TheSOSILibrary");
            Assert.Null(idCard.Password, "Forventede ikke password i ID Card");
            Assert.AreEqual(responseSigningVault.GetSystemCredentials(), idCard.SignedByCertificate, "Uventet Signing certificate");
            Assert.Null(idCard.Username, "Forventede ikke username i ID Card");
            Assert.AreEqual("1.0.1", idCard.Version, "Uventet version");

            SystemInfo systemInfo = idCard.SystemInfo;
            Assert.AreEqual("Korsbæk Kommunes IT systemer", systemInfo.ItSystemName, "Uventet IT System Name");
            Assert.AreEqual("20301823", systemInfo.CareProvider.Id, "Uventet Care Provider Id");
            Assert.AreEqual("Korsbæk Kommune", systemInfo.CareProvider.OrgName, "Uventet Care Provider Organization Name");
            Assert.AreEqual(SubjectIdentifierType.medcomcvrnumber, systemInfo.CareProvider.Type, "Uventet Care Provider Id Type");

            UserInfo userInfo2 = idCard.UserInfo;
            Assert.Null(userInfo2.AuthorizationCode, "Uventet Authorization Code");
            Assert.AreEqual("1010734321", userInfo2.Cpr, "Uventet CPR");
            Assert.Null(userInfo2.Email, "Forventede ikke Email i ID card");
            Assert.AreEqual("NSTSStreAnull", userInfo2.GivenName, "Uventet Given Name");
            Assert.Null(userInfo2.Occupation, "Forventede ikke Occupation i ID card");
            Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:41002:value:SundAssistR2", userInfo2.Role, "Uventet Role");
            Assert.AreEqual("Mortensen", userInfo2.SurName, "Uventet Sur Name");

            Assert.False(idCard.IsValidInTime, "Forventede ikke ID card var valid");

            idCard.ValidateSignature();
            idCard.ValidateSignatureAndTrust();
        }
    }
}
