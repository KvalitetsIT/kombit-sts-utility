using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using System;
using System.Xml.Linq;

namespace SealTest.Model
{
	class OIO2BSTCitizenTest
	{
		public DateTime validTo;
		public ICredentialVault mocesVault;
		public ICredentialVault vocesVault;

		[SetUp]
		public void Setup()
		{
			mocesVault = CredentialVaultTestUtil.GetMoces3CredentialVault();
			vocesVault = CredentialVaultTestUtil.GetVoces3CredentialVault();
			validTo = DateTime.Now.AddHours(2);
		}

		[Test]
		public void TestSAMLAssertionBuilder()
		{
			//GIVEN
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder();

			//WHEN
			OIO2BSTCitizenSAMLAssertion assertion = builder.Build();

			//THEN
			Validate(assertion);
		}

		[Test]
		public void TestEncryptDecrypt()
        {
			// Create bootstrap token and validate it.
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder();
			OIO2BSTCitizenSAMLAssertion assertion = builder.Build();

			Validate(assertion);

			// Encrypt bootstrap token
			var encryptionKey = CredentialVaultTestUtil.GetVoces3CredentialVault();

            XElement encryptedAssertion = new EncryptionUtil().EncryptAssertion(assertion.Dom, encryptionKey.GetSystemCredentials().PublicKey);
			Assert.NotNull(encryptedAssertion);
			Assert.AreEqual("EncryptedAssertion", encryptedAssertion.Name.LocalName);

			// Decrypt bootstrap token
			var decryptedAssertion = new EncryptionUtil().DecryptAssertion(encryptedAssertion, encryptionKey.GetSystemCredentials().PrivateKey);
			Assert.NotNull(decryptedAssertion);

			// Load the decrypted bootstrap token and validate it.
			assertion = new OIO2BSTCitizenSAMLAssertion(decryptedAssertion);
			Validate(assertion);
		}

		[Test]
		public void TestSAMLAssertionBuilderWithInvalidSigningCertificate()
		{
			//GIVEN
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder();
			builder.SigningVault = CredentialVaultTestUtil.GetCredentialVaultFromResource("untrusted.p12"); ;
			//WHEN
			OIO2BSTCitizenSAMLAssertion assertion = builder.Build();

			//THEN
			try
			{
				assertion.InternalValidateSignatureAndTrust(true, false, false);
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingAssuranceLevel()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setAssuranceLevel: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingCpr()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setCpr: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingIssuer()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setIssuer: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingNameId()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setNameId: false);
			builder.NameId = null;
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingAudience()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setAudience: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

        [Test]
		public void TestSAMLAssertionBuilderWithMissingHolderOfCertificate()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setHolderOfKeyCertificate: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionBuilderWithMissingSigningVault()
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = GetOIO2BSTCitizenSAMLAssertionBuilder(setSigningVault: false);
			try
			{
				builder.Build();
				Assert.Fail();
			}
			catch (ModelException)
			{
				// Empty
			}

		}

		[Test]
		public void TestSAMLAssertionFromElement()
		{
			//GIVEN
			var assertion = new OIO2BSTCitizenSAMLAssertion(XElement.Load(TestContext.CurrentContext.TestDirectory +
						  "/Resources/oiosaml-examples/OIOSAML3Assertion/OIO2BST-unencrypted-borger-example.xml"));
			validTo = new DateTime(2020, 11, 13, 0, 0, 0);

			//WHEN

			//THEN
			Validate(assertion, false);
		}

		private OIO2BSTCitizenSAMLAssertionBuilder GetOIO2BSTCitizenSAMLAssertionBuilder(bool setAssuranceLevel = true, 
			bool setCpr = true, 
			bool setIssuer = true, 
			bool setNotOnOrAfter = true, 
			bool setNameId = true, 
			bool setAudience = true, 
			bool setHolderOfKeyCertificate = true, 
			bool setSigningVault = true)
		{
			OIO2BSTCitizenSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIO2BSTCitizenSAMLAssertionBuilder();
			if(setNameId)
            {
				builder.NameId = "dk:gov:saml:attribute:CprNumberIdentifier:1802602810";
			}
			if(setHolderOfKeyCertificate)
            {
				builder.HolderOfKeyCertificate = mocesVault.GetSystemCredentials();
			}
			if(setAudience)
            {
				builder.Audience = "https://sts.sosi.dk/";
			}
			if(setSigningVault)
            {
				builder.SigningVault = vocesVault;

			}
			if (setIssuer)
			{
				builder.Issuer = "https://seblogin.nsi.dk/runtime/";
			}

			if (setNotOnOrAfter)
			{
				builder.NotOnOrAfter = validTo;
			}

			if (setAssuranceLevel)
			{
				builder.SetAssuranceLevel("3");
			}

			if (setCpr)
			{
				builder.Cpr = "1802602810";
			}

			return builder;
		}

		private void Validate(OIO2BSTCitizenSAMLAssertion assertion, bool validateCertificates = true)
		{
			Assert.AreEqual("OIO2BST_CITIZEN", assertion.AssertionType.ToString(), "Uventet type");

			Assert.AreEqual("https://seblogin.nsi.dk/runtime/", assertion.Issuer, "Uventet Issuer");
			Assert.AreEqual("dk:gov:saml:attribute:CprNumberIdentifier:1802602810", assertion.SubjectNameId, "Uventet Name ID");
			Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction, "Uventet Audience");
			//Tjekker tid uden MS
			var notOnOrAfterOffset = TimeZoneInfo.Local.IsDaylightSavingTime(assertion.NotOnOrAfter) ? 2 : 1;
			Assert.AreEqual(validTo.AddHours(notOnOrAfterOffset).ToString(), assertion.NotOnOrAfter.ToString(), "Uventet NotOnOrAfter date");
			Assert.AreEqual("3", assertion.AssuranceLevel, "Uventet Assurance Level");
			Assert.AreEqual("1802602810", assertion.Cpr, "Uventet CPR");

			//Test getMetoder der ikke er sat i GIVEN
			Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", assertion.SubjectNameIdFormat, "Uventet Name ID format");
			Assert.NotNull(assertion.Dom, "Forventede DOM objekt fra Assertion");
			Assert.NotNull(assertion.Id, "Forventede ID fra Assertion");
			Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion, "Uventet Spec version");
			Assert.IsNull(assertion.Uid, "Forventede ikke UUID fra Assertion");

			//Test getAttribute
			Assert.IsNull(assertion.GetAttributeValue("NonExistingAttribute"), "Uventet resultat af NonExistingAttribute");
			Assert.NotNull(assertion.GetAttributeValue(OIO2BSTCitizenSAMLAssertion.SamlCpr.Name), "Uventet resultat af OIO2BSTCitizenSAMLAssertion.Attribute.CPR");

			if (validateCertificates)
			{
				Assert.AreEqual(mocesVault.GetSystemCredentials(), assertion.GetHolderOfKeyCertificate(), "Uventet Holder Of Key Certificate");
				Assert.AreEqual(vocesVault.GetSystemCredentials(), assertion.SigningCertificate, "Uventet Signing Certificate");
				assertion.ValidateSignatureAndTrust();
			}
		}
	}
}