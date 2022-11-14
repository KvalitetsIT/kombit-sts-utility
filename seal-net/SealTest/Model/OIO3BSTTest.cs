using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using System;

namespace SealTest.Model
{
    class OIO3BSTTest
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

        private OIO3BSTSAMLAssertionBuilder GetOIO3BSTSAMLAssertionBuilder()
        {
            //GIVEN
            OIO3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIO3BSTSAMLAssertionBuilder();
            //Assertion
            builder.Issuer = "https://sts.nemlog-in.dk";
            //Subject
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            //Conditions
            builder.Audience = "https://sts.sosi.dk/";

            builder.NotOnOrAfter = validTo;
            //Signature
            builder.SigningVault = signingVault;
            //AttributeStatemnent
            //builder.setAssuranceLevel(NSIS.Substantial);
            builder.SetAssuranceLevel("NIST", "3");
            builder.Cpr = "1802602810";
            builder.Uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
            builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";

            return builder;
        }

        private OIO3BSTSAMLAssertionBuilder GetOIO3BSTSAMLAssertionBuilderWithoutAttributes()
        {
            //GIVEN
            OIO3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIO3BSTSAMLAssertionBuilder();
            //Assertion
            builder.Issuer = "https://sts.nemlog-in.dk";
            //Subject
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            //Conditions
            builder.Audience = "https://sts.sosi.dk/";

            builder.NotOnOrAfter = validTo;
            //Signature
            builder.SigningVault = signingVault;
            //AttributeStatemnent
            //builder.setAssuranceLevel(NSIS.Substantial);


            return builder;
        }

        private void Validate(OIOBSTSAMLAssertion assertion)
        {
            //System.out.println(XmlUtil.node2String(assertion.getDOM(), true, false));
            Assert.AreEqual(OIOBSTSAMLAssertion.Type.OIO3BST, assertion.AssertionType, "Uventet type");

            Assert.AreEqual("https://sts.nemlog-in.dk", assertion.Issuer, "Uventet Issuer");
            Assert.AreEqual("KorsbaekKommune\\MSK", assertion.SubjectNameId, "Uventet Name ID");
            Assert.AreEqual(holderOfKeyVault.GetSystemCredentials(), assertion.GetHolderOfKeyCertificate(), "Uventet Holder Of Key Certificate");
            Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction, "Uventet Audience");
            //Tjekker tid uden MS
            Assert.NotNull(assertion.NotOnOrAfter, "Uventet NotOnOrAfter date");
            Assert.AreEqual(signingVault.GetSystemCredentials(), assertion.SigningCertificate, "Uventet Signing Certificate");
            Assert.AreEqual("3", assertion.AssuranceLevel, "Uventet Assurance Level");
            Assert.AreEqual("1802602810", assertion.Cpr, "Uventet CPR");
            Assert.Null(assertion.RidNumberIdentifier, "Forventede ikke RID i Assertion");
            Assert.AreEqual("20301823", assertion.CvrNumberIdentifier, "Uventet CVR");
            Assert.AreEqual("Korsbæk Kommune", assertion.OrganizationName, "Uventet Organization Name");
            Assert.Null(assertion.BasicPrivileges, "Forventede ikke Privileges fra Assertion");

            //Test getMetoder der ikke er sat i GIVEN
            Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", assertion.SubjectNameIdFormat, "Uventet Name ID format");
            Assert.NotNull(assertion.Dom, "Forventede DOM objekt fra Assertion");
            Assert.NotNull(assertion.Id, "Forventede ID fra Assertion");
            Assert.AreEqual("OIO-SAML-3.0", assertion.SpecVersion, "Uventet Spec version");
            Assert.AreEqual("urn:uuid:323e4567-e89b-12d3-a456-426655440000", assertion.Uid, "Uventet UUID fra Assertion");

            //Test getAttribute
            Assert.Null(assertion.GetAttributeValue("NonExistingAttribute"), "Uventet resultat af NonExistingAttribute");
            Assert.Null(assertion.GetAttributeValue(OioSamlAttributes.CvrNumber), "Uventet resultat af OIO3BSTSAMLAssertion.Attribute.CVR");
            Assert.NotNull(assertion.GetAttributeValue(OioSaml3Attributes.Cvr), "Uventet resultat af OIOH3BSTSAMLAssertion.Attribute.CVR");
        }


        [Test]
        public void TestSAMLAssertionBuilder()
        {
            //GIVEN
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();

            //WHEN
            OIO3BSTSAMLAssertion assertion = builder.Build();

            //THEN
            Validate(assertion);
        }

        [Test]
        public void TestBuildValidatesAssertion()
        {
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();
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
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();
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
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();
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
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();
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
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilder();
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


        [Test]
        public void TestMissingAttributesOrganizationName()
        {
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilderWithoutAttributes();
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
            OIO3BSTSAMLAssertionBuilder builder = GetOIO3BSTSAMLAssertionBuilderWithoutAttributes();
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
    }
}
