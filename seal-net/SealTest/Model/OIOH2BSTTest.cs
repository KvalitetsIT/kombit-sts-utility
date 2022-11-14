using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using System;
using System.Linq;

namespace SealTest.Model
{
    class OIOH2BSTTest
	{
		public DateTime validTo;
		public ICredentialVault holderOfKeyVault;
		public ICredentialVault signingVault;

		[SetUp]
		public void Setup()
		{
			holderOfKeyVault = CredentialVaultTestUtil.GetFoces3CredentialVault();
			signingVault = CredentialVaultTestUtil.GetVoces3CredentialVault();

			validTo = DateTime.Now.AddHours(2);
		}

		private OIOH2BSTSAMLAssertionBuilder GetOIOH2BSTSAMLAssertionBuilder(bool withPrivileges)
		{
			//GIVEN
			OIOH2BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionBuilder();
			//Assertion
			builder.Issuer = "https://seblogin.nsi.dk/runtime/";
			//Subject
			builder.NameId = "KorsbaekKommune\\MSK";
			builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
			//Conditions
			builder.Audience = "https://sts.sosi.dk/";

			//builder.NotOnOrAfter = validTo;
			//Signature
			builder.SigningVault = signingVault;
			//AttributeStatemnent
			builder.SetAssuranceLevel("NIST", "3");
			builder.Cpr = "1802602810";
			builder.Rid = "1231593107593";
			builder.Cvr = "20301823";
			builder.OrganizationName = "Korsbæk Kommune";

			if (withPrivileges)
			{
				//privilege
				BasicPrivilegesDOMBuilder privilegesBuilder = new();
				privilegesBuilder.AddPrivilege("urn:dk:gov:saml:cvrNumberIdentifier:20301823", "urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3");
				builder.Privileges = privilegesBuilder;
			}

			return builder;
		}

		private OIOH2BSTSAMLAssertionBuilder GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes()
		{
			//GIVEN
			OIOH2BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionBuilder();
			//Assertion
			builder.Issuer = "https://seblogin.nsi.dk/runtime/";
			//Subject
			builder.NameId = "KorsbaekKommune\\MSK";
			builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
			//Conditions
			builder.Audience = "https://sts.sosi.dk/";

            builder.NotOnOrAfter = validTo;
            //Signature
            builder.SigningVault = signingVault;
			//AttributeStatemnent

			//privilege
			BasicPrivilegesDOMBuilder privilegesBuilder = new();
			privilegesBuilder.AddPrivilege("urn:dk:gov:saml:cvrNumberIdentifier:20301823", "urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3");
			builder.Privileges = privilegesBuilder;

			return builder;
		}


		private void Validate(OIOBSTSAMLAssertion assertion, bool withPrivileges)
		{
			Assert.AreEqual(OIOBSTSAMLAssertion.Type.OIOH2BST, assertion.AssertionType, "Uventet type");

			Assert.AreEqual("https://seblogin.nsi.dk/runtime/", assertion.Issuer, "Uventet Issuer");
			Assert.AreEqual("KorsbaekKommune\\MSK", assertion.SubjectNameId, "Uventet Name ID");
			Assert.AreEqual(holderOfKeyVault.GetSystemCredentials(), assertion.GetHolderOfKeyCertificate(), "Uventet Holder Of Key Certificate");
			Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction, "Uventet Audience");

			//Tjekker tid uden MS
			Assert.NotNull(assertion.NotOnOrAfter, "Uventet NotOnOrAfter date");
			Assert.AreEqual(signingVault.GetSystemCredentials(), assertion.SigningCertificate, "Uventet Signing Certificate");
			Assert.AreEqual("3", assertion.AssuranceLevel, "Uventet Assurance Level");
			Assert.AreEqual("1802602810", assertion.Cpr, "Uventet CPR");
			Assert.AreEqual("1231593107593", assertion.RidNumberIdentifier, "Uventet RID");
			Assert.AreEqual("20301823", assertion.CvrNumberIdentifier, "Uventet CVR");
			Assert.AreEqual("Korsbæk Kommune", assertion.OrganizationName, "Uventet Organization Name");
			if (withPrivileges)
			{
				Assert.NotNull(assertion.BasicPrivileges, "Forventede Privileges fra Assertion");
				Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3", assertion.BasicPrivileges.getPrivileges("urn:dk:gov:saml:cvrNumberIdentifier:20301823").First(), "Uventet Privilege");
			}
			else
			{
				Assert.Null(assertion.BasicPrivileges, "Forventede ikke Privileges fra Assertion");
			}

			//Test getMetoder der ikke er sat i GIVEN
			Assert.AreEqual("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified", assertion.SubjectNameIdFormat, "Uventet Name ID format");
			Assert.NotNull(assertion.Dom, "Forventede DOM objekt fra Assertion");
			Assert.NotNull(assertion.Id, "Forventede ID fra Assertion");
			Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion, "Uventet Spec version");
			Assert.Null(assertion.Uid, "Forventede ikke UUID fra Assertion");

			//Test getAttribute
			Assert.Null(assertion.GetAttributeValue("NonExistingAttribute"), "Uventet resultat af NonExistingAttribute");
			Assert.NotNull(assertion.GetAttributeValue(OIOH2BSTSAMLAssertion.SamlCvr.Name), "Uventet resultat af OIOH2BSTSAMLAssertion.Attribute.CPR");
			Assert.Null(assertion.GetAttributeValue(OIOH3BSTSAMLAssertion.SamlCvr.Name), "Uventet resultat af OIOH3BSTSAMLAssertion.Attribute.CPR");
			Assert.Null(assertion.GetAttributeValue(OIO3BSTSAMLAssertion.SamlCvr.Name), "Uventet resultat af OIO3BSTSAMLAssertion.Attribute.CPR");
		}

		[Test]
		public void TestSAMLAssertionBuilder()
		{
			//GIVEN
			var withPrivileges = true;
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(withPrivileges);

			//WHEN
			OIOH2BSTSAMLAssertion assertion = builder.Build();

			//THEN
			Validate(assertion, withPrivileges);
		}

        [Test]
        public void TestBuildValidatesAssertion()
        {
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(false);
			builder.HolderOfKeyCertificate = null;
			try
			{
				builder.Build();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("holderOfCertificate is mandatory - but was null Value."), e.Message);
			}
		}

        [Test]
		public void TestMissingHolderOfKey()
		{
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(false);
			builder.HolderOfKeyCertificate = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("holderOfCertificate is mandatory - but was null Value."), e.Message);
			}
		}

		[Test]
		public void TestMissingSigningVault()
		{
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(false);
			builder.SigningVault = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("signingVault is mandatory - but was null Value"), e.Message);
			}
		}

		[Test]
		public void TestMissingNameId()
		{
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(false);
			builder.NameId = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("nameId is mandatory - but was an empty String"), e.Message);
			}
		}

		[Test]
		public void TestMissingAudience()
		{
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(false);
			builder.Audience = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("audience is mandatory - but was an empty String"), e.Message);
			}
		}


        [Test]
        public void TestMissingAttributesOrganizationName()
        {
			var builder = GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NIST", "3");
			builder.Cpr = "1802602810";
			builder.Rid = "1231593107593";
			builder.Cvr = "20301823";

            try
            {
                builder.ValidateBeforeBuild();
                Assert.Fail("Validation should fail for missing attribute organization");
            }
            catch (ModelException e)
            {
                Assert.True(e.Message.Contains("attribute:urn:oid:2.5.4.10 is mandatory - but was an empty String."), e.Message);
            }
        }

		[Test]
		public void TestMissingAttributesCvr()
		{
			var builder = GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NIST", "3");
			builder.Cpr = "1802602810";
			builder.Rid = "1231593107593";
            builder.OrganizationName = "Korsbæk Kommune";

            try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail for missing attribute organization");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:dk:gov:saml:attribute:CvrNumberIdentifier is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMissingAttributesRid()
		{
			var builder = GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NIST", "3");
			builder.Cpr = "1802602810";
			builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";

            try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("One of RID or UUID must be present.");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("One of RID or UUID must be present."), e.Message);
			}
		}

		[Test]
		public void TestMissingAttributesCpr()
		{
			var builder = GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NIST", "3");
			builder.Rid = "1231593107593";
			builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";

            try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail for missing attribute cpr");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:dk:gov:saml:attribute:CprNumberIdentifier is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMissingAttributesAssuranceLevel()
		{
			var builder = GetOIOH2BSTSAMLAssertionBuilderWithoutAttributes();
			builder.Cpr = "1802602810";
			builder.Rid = "1231593107593";
			builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";

            try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail for missing attribute assurance level");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:dk:gov:saml:attribute:AssuranceLevel is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMinimalSAMLAssertionBuilder()
		{
			//GIVEN
			bool withPrivileges = false;
			OIOH2BSTSAMLAssertionBuilder builder = GetOIOH2BSTSAMLAssertionBuilder(withPrivileges);

			//WHEN
			OIOH2BSTSAMLAssertion assertion = builder.Build();

			//THEN
			Validate(assertion, withPrivileges);
		}

	}
}
