using System;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Federation;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using dk.nsi.seal.pki;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateSuiteOces2))]
    [TestFixture(typeof(CertificateSuiteOces3))]
    public class OioSamlFactoryTest<CERTIFICATE_SUITE> : AbstractTest where CERTIFICATE_SUITE : ICertificateSuite, new()
	{
        private ICertificateSuite CertificateSuite;

		[SetUp]
		public void Init()
		{
            CertificateSuite = new CERTIFICATE_SUITE();
		}

		[TearDown]
		public void TearDown()
		{
		}

		private OioSamlAssertion ParseOioSamlAssertion()
		{
			return new OioSamlAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
								 "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-2.xml"));
		}

		[Test]
		public void TestOioSamlToIdCardRequest()
		{
            var credentialVault = CertificateSuite.VocesValid.CredentialVault;
			var domBuilder = OIOSAMLFactory.CreateOiosamlAssertionToIdCardRequestDomBuilder();
            domBuilder.SigningVault = credentialVault;
            domBuilder.OioSamlAssertion = ParseOioSamlAssertion();
			domBuilder.ItSystemName = "EMS";
			domBuilder.UserAuthorizationCode = "2345C";
			domBuilder.UserEducationCode = "7170";
			domBuilder.UserGivenName = "Fritz";
			domBuilder.UserSurName = "Müller";
            var requestDoc = domBuilder.Build();

			var assertionToIdCardRequest = OIOSAMLFactory.CreateOioSamlAssertionToIdCardRequestModelBuilder().Build(requestDoc);
			Assert.AreEqual("EMS", assertionToIdCardRequest.ItSystemName);
			Assert.AreEqual("2345C", assertionToIdCardRequest.UserAuthorizationCode);
			Assert.AreEqual("7170", assertionToIdCardRequest.UserEducationCode);
			Assert.AreEqual("Fritz", assertionToIdCardRequest.UserGivenName);
			Assert.AreEqual("Müller", assertionToIdCardRequest.UserSurName);
			Assert.AreEqual("http://sosi.dk", assertionToIdCardRequest.AppliesTo);
			Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", assertionToIdCardRequest.Action);
			assertionToIdCardRequest.ValidateSignature();
            assertionToIdCardRequest.ValidateSignatureAndTrust();
			Assert.AreEqual(credentialVault.GetSystemCredentials(), assertionToIdCardRequest.GetSigningCertificate());

			var assertion = assertionToIdCardRequest.OioSamlAssertion;
			Assert.AreEqual("25520041", assertion.CvrNumberIdentifier);
			Assert.AreEqual("_5a49e560-5312-4237-8f32-2ed2b58cfcf7", assertion.Id);
		}


		[Test]
		public void ValidateNemLoginAssertion()
		{
			var assertionXElement = XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
			                                      "/Resources/oiosaml-examples/NemLog-in_assertion_valid_signature.xml");
			var assertion = new OioSamlAssertion(assertionXElement);
			assertion.ValidateSignatureAndTrust();
			Assert.AreEqual("3", assertion.AssuranceLevel);
			Assert.AreEqual("25450442", assertion.CvrNumberIdentifier);
			Assert.AreEqual("27304742", assertion.RidNumberIdentifier);
		}

		private UserIdCard CreateIdCard() 
		{
			SOSIFactory sosiFactory = new(null, new CredentialVaultSignatureProvider(CredentialVaultTestUtil.GetCredentialVault()));  
			CareProvider careProvider = new(SubjectIdentifierType.medcomcvrnumber, "30808460", "Lægehuset på bakken");
			UserInfo userInfo = new("1111111118", "Hans", "Dampf", null, null, "7170", "341KY");
			String alternativeIdentifier = new CertificateInfo(CredentialVaultTestUtil.GetCredentialVault().GetSystemCredentials()).ToString();
			var userIdCard = SOSIFactory.CreateNewUserIdCard("IT-System", userInfo, careProvider, AuthenticationLevel.MocesTrustedUser, null, null, null, alternativeIdentifier);
			userIdCard.Sign<Assertion>(sosiFactory.SignatureProvider);
			return userIdCard;
		}

        [Test]
		public void TestUserIdCardToOioSamlRequest()
		{
            var credentialVault = CertificateSuite.VocesValid.CredentialVault;
			var domBuilder = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestDomBuilder();
            domBuilder.SigningVault = credentialVault; 
			domBuilder.Audience = "Sundhed.dk";
			var idCard = CreateIdCard();
			domBuilder.IdCardAssertion =  idCard.Xassertion;
			var requestDoc = domBuilder.Build();

			var assertionRequest = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestModelBuilder().Build(requestDoc);
			Assert.AreEqual("Sundhed.dk", assertionRequest.AppliesTo);
			Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", assertionRequest.Action);
			assertionRequest.ValidateSignature();
			assertionRequest.ValidateSignatureAndTrust();
			try
			{
				assertionRequest.ValidateSignatureAndTrust();
			}
			catch (ModelException e)
			{
				Assert.AreEqual("The certificate that signed the security token is not trusted!", e.Message);
			}
			Assert.AreEqual(credentialVault.GetSystemCredentials(), assertionRequest.GetSigningCertificate());

			Assert.IsTrue(idCard.Equals(assertionRequest.UserIdCard));
            assertionRequest.UserIdCard.ValidateSignature();
            assertionRequest.UserIdCard.ValidateSignatureAndTrust();
			try
			{
				assertionRequest.UserIdCard.ValidateSignatureAndTrust(new SosiFederation(new CrlCertificateStatusChecker()));
			}
			catch (ModelException e)
			{
				Assert.AreEqual("The certificate that signed the security token is not trusted!", e.Message);
			}
		}

        [Test]
        public void TestIdCardToOioSamlRequestIdCardInHeader()
		{
			var domBuilder = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestDomBuilder();
			domBuilder.PlaceIdCardInSoapHeader = true;
			domBuilder.Audience = "Sundhed.dk";
			var idCard = CreateIdCard();
			domBuilder.IdCardAssertion = idCard.Xassertion;
			var requestDoc = domBuilder.Build();

			var assertionRequest = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestModelBuilder().Build(requestDoc);
			Assert.AreEqual("Sundhed.dk", assertionRequest.AppliesTo);
			Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", assertionRequest.Action);
			try
			{
				assertionRequest.ValidateSignature();
                Assert.Fail();
            }
			catch (ModelBuildException e)
			{
				Assert.AreEqual("Could not find Liberty signature element", e.Message);
			}
			Assert.Null(assertionRequest.GetSigningCertificate());

			Assert.AreEqual(idCard, assertionRequest.UserIdCard);
			assertionRequest.UserIdCard.ValidateSignature();
			assertionRequest.UserIdCard.ValidateSignatureAndTrust(); 
			try
			{
				assertionRequest.UserIdCard.ValidateSignatureAndTrust(new SosiFederation(new CrlCertificateStatusChecker()));
			}
			catch (ModelException e)
			{
				Assert.AreEqual("The certificate that signed the security token is not trusted!", e.Message);
			}
		}

		[Test]
		public void TestIdCardToOioSamlRequestMissingIdCard()
		{
			var domBuilder = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestDomBuilder();
			domBuilder.PlaceIdCardInSoapHeader = true;
			domBuilder.Audience = "Sundhed.dk";
			var idCard = CreateIdCard(); 
			domBuilder.IdCardAssertion = idCard.Xassertion;
			var requestDoc = domBuilder.Build();

			var idcardAssertion = requestDoc.Descendants(SamlTags.Assertion.Ns + SamlTags.Assertion.TagName).FirstOrDefault();//.getElementsByTagNameNS(NameSpaces.SAML2ASSERTION_SCHEMA, SAMLTags.ASSERTION).item(0);
			idcardAssertion.Remove();//.removeChild(idcardAssertion);

			var assertionRequest = OIOSAMLFactory.CreateIdCardToOioSamlAssertionRequestModelBuilder().Build(requestDoc);
			Assert.AreEqual("Sundhed.dk", assertionRequest.AppliesTo);
			Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", assertionRequest.Action);
			try
			{
				assertionRequest.ValidateSignature();
                Assert.Fail();
			}
			catch (ModelBuildException e)
			{
				Assert.AreEqual("Could not find Liberty signature element", e.Message);
			}
			Assert.Null(assertionRequest.GetSigningCertificate());

			try
			{
				Assert.AreEqual(idCard, assertionRequest.UserIdCard);
			}
			catch (ModelException e)
			{
				Assert.AreEqual("Malformed request: IDCard could not be found!", e.Message);
			}
		}

		[Test]
		public void TestOioSamlAssertionBuilder()
		{
			var idCard = CreateIdCard();
			var assertion = CreateOioSamlAssertion(idCard);

			AssertOioSamlAssertion(assertion, idCard);
		}

		private void AssertOioSamlAssertion(OioSamlAssertion assertion, UserIdCard idCard)
		{
			Assert.AreEqual("42634739", assertion.RidNumberIdentifier);
			Assert.AreEqual("CN=TRUST2408 Systemtest XXII CA, O=TRUST2408, C=DK", assertion.CertificateIssuer);
			Assert.IsFalse(assertion.IsYouthCertificate);
			Assert.AreEqual("5BAD375E", assertion.CertificateSerial);
			Assert.AreEqual("CVR:30808460-RID:42634739", assertion.Uid);
			Assert.IsNotNull(assertion.NotOnOrAfter);
			Assert.AreEqual("http://sundhed.dk/saml/SAMLAssertionConsumer", assertion.Recipient);
			Assert.AreEqual(idCard, assertion.UserIdCard);
			assertion.ValidateSignatureAndTrust();
		}

		private OioSamlAssertion CreateOioSamlAssertion(UserIdCard idCard)
		{
			var now = DateTimeEx.UtcNowRound;
			var builder = OIOSAMLFactory.CreateOioSamlAssertionBuilder();
			builder.SigningVault = CredentialVaultTestUtil.GetVocesCredentialVault();
			builder.Issuer = "Test STS";
			builder.UserIdCard = idCard;
			builder.NotBefore = now;
			builder.NotOnOrAfter = now.AddHours(1);
			builder.AudienceRestriction = "http://sundhed.dk";
			builder.RecipientUrl = "http://sundhed.dk/saml/SAMLAssertionConsumer";
			builder.DeliveryNotOnOrAfter = now.AddMinutes(5);
			builder.IncludeIdCardAsBootstrapToken = true;
			builder.UserCertificate = CertificateSuite.MocesCprValid.Certificate;
            return builder.Build();
		}

	}
}
