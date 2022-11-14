using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using System;
using System.Linq;

namespace SealTest.Model
{
    class OIOH3BSTTest
	{
		public DateTime validTo;
		public ICredentialVault holderOfKeyVault;
		public ICredentialVault signingVault;

		[SetUp]
		public void Setup()
		{
			holderOfKeyVault = CredentialVaultTestUtil.GetFoces3CredentialVault();
			signingVault = CredentialVaultTestUtil.GetVoces3CredentialVault();

			validTo = DateTimeOffset.UtcNow.AddHours(2).DateTime;
		}

		private OIOH3BSTSAMLAssertionBuilder GetOIOH3BSTSAMLAssertionBuilder(bool withPrivileges)
		{
			//GIVEN
			OIOH3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH3BSTSAMLAssertionBuilder();
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
			builder.SetAssuranceLevel("NSIS", "Substantial");
			builder.Uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
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


		private OIOH3BSTSAMLAssertionBuilder GetOIOH3BSTSAMLAssertionBuilderWithoutAttributes()
		{
			//GIVEN
			OIOH3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH3BSTSAMLAssertionBuilder();
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

			//privilege
			BasicPrivilegesDOMBuilder privilegesBuilder = new();
			privilegesBuilder.AddPrivilege("urn:dk:gov:saml:cvrNumberIdentifier:20301823", "urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3");
			builder.Privileges = privilegesBuilder;

			return builder;
		}

		private void Validate(OIOBSTSAMLAssertion assertion, bool withPrivileges)
		{
			Assert.AreEqual(typeof(OIOH3BSTSAMLAssertion), assertion.GetType());
			OIOH3BSTSAMLAssertion oioh3bstAssertion = (OIOH3BSTSAMLAssertion)assertion;
			Assert.AreEqual(OIOBSTSAMLAssertion.Type.OIOH3BST, oioh3bstAssertion.AssertionType, "Uventet type");
			Assert.AreEqual("OIO-SAML-3.0", oioh3bstAssertion.SpecVersion, "Uventet spec version");
			Assert.AreEqual("OIO-SAML-H-3.0", oioh3bstAssertion.SpecVersionAdditional, "Uventet additional spec version");

			Assert.AreEqual("https://seblogin.nsi.dk/runtime/", oioh3bstAssertion.Issuer, "Uventet Issuer");
			Assert.AreEqual("KorsbaekKommune\\MSK", oioh3bstAssertion.SubjectNameId, "Uventet Name ID");
			Assert.AreEqual(holderOfKeyVault.GetSystemCredentials(), oioh3bstAssertion.GetHolderOfKeyCertificate(), "Uventet Holder Of Key Certificate");
			Assert.AreEqual("https://sts.sosi.dk/", oioh3bstAssertion.AudienceRestriction, "Uventet Audience");
			//Tjekker tid uden MS
			Assert.NotNull(oioh3bstAssertion.NotOnOrAfter, "Uventet NotOnOrAfter date");
			Assert.AreEqual(signingVault.GetSystemCredentials(), oioh3bstAssertion.SigningCertificate, "Uventet Signing Certificate");
			Assert.AreEqual("Substantial", oioh3bstAssertion.AssuranceLevel, "Uventet Assurance Level");
			Assert.Null(oioh3bstAssertion.Cpr, "Forventer ikke CPR i OIOH3BST");
			Assert.Null(oioh3bstAssertion.RidNumberIdentifier, "Forventer ikke RID i OIOH3BST");
			Assert.AreEqual("20301823", oioh3bstAssertion.CvrNumberIdentifier, "Uventet CVR");
			Assert.AreEqual("Korsbæk Kommune", oioh3bstAssertion.OrganizationName, "Uventet Organization Name");
			if (withPrivileges)
			{
				Assert.NotNull(oioh3bstAssertion.BasicPrivileges, "Forventede Privileges fra Assertion");
				Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3", oioh3bstAssertion.BasicPrivileges.getPrivileges("urn:dk:gov:saml:cvrNumberIdentifier:20301823").First(), "Uventet Privilege");
			}
			else
			{
				Assert.Null(oioh3bstAssertion.BasicPrivileges, "Forventede ikke Privileges fra Assertion");
			}

			//Test getMetoder der ikke er sat i GIVEN
			Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", oioh3bstAssertion.SubjectNameIdFormat, "Uventet Name ID format");
			Assert.NotNull(oioh3bstAssertion.Dom, "Forventede DOM objekt fra Assertion");
			Assert.NotNull(oioh3bstAssertion.Id, "Forventede ID fra Assertion");
			Assert.AreEqual("OIO-SAML-3.0", oioh3bstAssertion.SpecVersion, "Uventet Spec version");
			Assert.AreEqual("urn:uuid:323e4567-e89b-12d3-a456-426655440000", oioh3bstAssertion.Uid, "Uventet uuid");

			//Test getAttribute
			Assert.Null(oioh3bstAssertion.GetAttributeValue("NonExistingAttribute"), "Uventet resultat af NonExistingAttribute");
			Assert.Null(oioh3bstAssertion.GetAttributeValue(OIOH2BSTSAMLAssertion.SamlSpecVer.Name), "Uventet resultat af OIOH2BSTSAMLAssertion.Attribute.CPR");
			Assert.NotNull(oioh3bstAssertion.GetAttributeValue(OIOH3BSTSAMLAssertion.SamlSpecVer.Name), "Uventet resultat af OIOH3BSTSAMLAssertion.Attribute.CPR");
			Assert.NotNull(oioh3bstAssertion.GetAttributeValue(OIO3BSTSAMLAssertion.SamlSpecVer.Name), "Uventet resultat af OIO3BSTSAMLAssertion.Attribute.CPR");
		}


		[Test]
		public void TestMinimalSAMLAssertionBuilder()
		{
			//GIVEN
			bool withPrivileges = false;
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(withPrivileges);

			//WHEN
			OIOH3BSTSAMLAssertion assertion = builder.Build();

			//THEN
			Validate(assertion, withPrivileges);
		}

		[Test]
		public void TestSAMLAssertionBuilder()
		{
			//GIVEN
			bool withPrivileges = true;
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(withPrivileges);

			//WHEN
			OIOH3BSTSAMLAssertion assertion = builder.Build();

			//THEN
			Validate(assertion, withPrivileges);
        }

		[Test]
		public void TestBuildValidatesAssertion()
		{
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
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
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
			builder.HolderOfKeyCertificate = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("holderOfCertificate is mandatory - but was null"), e.Message);
			}
		}
		[Test]
		public void TestMissingSigningVault()
		{
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
			builder.SigningVault = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("signingVault is mandatory - but was null"), e.Message);
			}
		}

		[Test]
		public void TestMissingNameId()
		{
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
			builder.NameId = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("nameId is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMissingAudience()
		{
			OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
			builder.Audience = null;
			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("audience is mandatory - but was an empty String."), e.Message);
			}
		}

		//[Test]
		//public void testMissingNotOnOrAfter()
		//{
		//	OIOH3BSTSAMLAssertionBuilder builder = GetOIOH3BSTSAMLAssertionBuilder(false);
		//	builder.setNotOnOrAfter(null);
		//	try
		//	{
		//		builder.validateBeforeBuild();
		//		fail("Validation should fail");
		//	}
		//	catch (ModelException e)
		//	{
		//		assertTrue(e.getMessage(), e.getMessage().contains("notOnOrAfter is mandatory - but was null"));
		//	}
		//}

		[Test]
		public void TestMissingAttributesOrganizationName()
		{
			var builder = GetOIOH3BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NSIS", "Substantial");
			builder.Uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
			builder.Cvr = "20301823";

			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:https://data.gov.dk/model/core/eid/professional/orgName is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMissingAttributesCvr()
		{
			var builder = GetOIOH3BSTSAMLAssertionBuilderWithoutAttributes();
			builder.SetAssuranceLevel("NSIS", "Substantial");
			builder.Uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
			builder.OrganizationName = "Korsbæk Kommune";

			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:https://data.gov.dk/model/core/eid/professional/cvr is mandatory - but was an empty String."), e.Message);
			}
		}

		[Test]
		public void TestMissingAttributesAssuranceLevel()
		{
			var builder = GetOIOH3BSTSAMLAssertionBuilderWithoutAttributes();
			builder.Uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
			builder.Cvr = "20301823";
			builder.OrganizationName = "Korsbæk Kommune";

			try
			{
				builder.ValidateBeforeBuild();
				Assert.Fail("Validation should fail");
			}
			catch (ModelException e)
			{
				Assert.True(e.Message.Contains("attribute:https://data.gov.dk/concept/core/nsis/loa is mandatory - but was an empty String."), e.Message);
			}
		}
    }
}

