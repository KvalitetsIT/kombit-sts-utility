using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Model.ModelBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using SealTest.Certificate;
using System;
using System.Linq;
using System.Xml.Linq;

namespace SealTest.Model
{
    class OIOH2BSTSAMLAssertionToIDCardRequestTest
    {
        private OIOBSTSAMLAssertionToIDCardRequestModelBuilder requestModelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardRequestModelBuilder();
        private readonly string uuid = "urn:uuid:323e4567-e89b-12d3-a456-426655440000";
        private readonly string audience = "https://sts.sosi.dk/";
        private readonly DateTime validTo = DateTime.Now.AddHours(2);
        private readonly string cvr = "20301823";
        private readonly string organizationName = "Korsbæk Kommune";
        private readonly string cpr = "0707764321";
        private readonly string userRole = "7170";
        private readonly string authorizationCode = "ZXCVB";
        private static string issuer;
        private static string assuranceLevel;

        private readonly ICredentialVault holderOfKeyVault = new InMemoryCredentialVault(new CertificateOces3Foces().Certificate);
        private readonly ICredentialVault assertionSigningVault = new InMemoryCredentialVault(new CertificateOces3Voces().Certificate);

        private AbstractOIOBSTSAMLAssertionBuilder<OIOH2BSTSAMLAssertion> assertionBuilder;
        private OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> requestBuilder;

        private readonly OIOBSTSAMLAssertion.Type tokenType = OIOBSTSAMLAssertion.Type.OIOH2BST;


        [SetUp]
        public void Setup()
        {
            assertionBuilder = CreateOIOH2BSTSAMLAssertionBuilder(holderOfKeyVault, assertionSigningVault);
            requestBuilder = requestBuilderFromAssertionBuilder(holderOfKeyVault, assertionBuilder);
        }

        [Test]
        public void TestValidRequest()
        {
            //GIVEN
            var requestDocument = requestBuilder.Build();

            //WHEN
            var request = requestModelBuilder.Build(requestDocument);

            //THEN
            request.ValidateSignature();
            request.ValidateSignatureAndTrust();
            request.ValidateHolderOfKeyRelation();
            
            Assert.AreEqual("https://fmk", request.AppliesTo, "Uventet Audience");
            Assert.AreEqual("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", request.Action, "Uventet Action");
            Assert.NotNull(request.Context, "Forventede Context uuid i Request");
            Assert.AreEqual("Korsbæk Kommunes IT systemer", request.GetITSystemName(), "Uventet IT System Name");
            Assert.NotNull(request.MessageID, "Forventede Message uuid i Request");
            Assert.AreEqual(holderOfKeyVault.GetSystemCredentials(), request.GetSigningCertificate(), "Uventet Signing Certificate");
            Assert.AreEqual("Mads_Skjern", request.GetSubjectNameId(), "Uventet Subject Name ID");
            Assert.AreEqual(userRole, request.GetUserRole(), "Uventet User Role");
            Assert.AreEqual(authorizationCode, request.GetUserAuthorizationCode(), "Uventet Authorization Code");
            Assert.AreEqual("medcom:ITSystemName=Korsbæk Kommunes IT systemer,medcom:UserAuthorizationCode=ZXCVB,medcom:UserRole=7170,sosi:SubjectNameID=Mads_Skjern", String.Join(",", request.getClaimMap().Select(x => $"{x.Key}={x.Value}").OrderBy(x => x)), "Uventet indhold af Claims");

            Validate(request.OIOBSTSAMLAssertion);
        }

        [Test]
        public void TestInvalidHolderOfKeyRelation()
        {
            //GIVEN
            assertionBuilder.HolderOfKeyCertificate = assertionSigningVault.GetSystemCredentials();
            var requestDocument = requestBuilder.Build();

            //WHEN
            var request = requestModelBuilder.Build(requestDocument);

            //THEN
            try
            {
                request.ValidateHolderOfKeyRelation();
                Assert.Fail("holder-of-key validation should fail");
            }
            catch (Exception e)
            {
                Assert.True(e.Message.Contains("Signing certificate and holder-of-key certificate relation do not match"), e.Message);
            }
        }

        [Test]
        public void TestUntrustedSignature()
        {
            //GIVEN
            assertionBuilder.HolderOfKeyCertificate = assertionSigningVault.GetSystemCredentials();
            requestBuilder.SigningVault = CredentialVaultTestUtil.GetCredentialVaultFromResource("untrusted.p12"); ;
            var requestDocument = requestBuilder.Build();

            //WHEN
            var request = requestModelBuilder.Build(requestDocument);

            //THEN
            try
            {
                request.InternalValidateSignatureAndTrust(true, false, false);
                Assert.Fail("validation should fail");
            }
            catch (Exception e)
            {
                Assert.True(e.Message.Contains("The certificate that signed the security token is not trusted!"), e.Message);
            }
        }

        [Test]
        public void TestBrokenMessageSignature()
        {
            //GIVEN
            XDocument requestDocument = requestBuilder.Build();
            
            var itSystemClaim = requestDocument.Descendants(SoapTags.Envelope.Ns + SoapTags.Envelope.TagName)
                                   .Descendants(SoapTags.Body.Ns + SoapTags.Body.TagName)
                                   .Descendants(WstTags.RequestSecurityToken.Ns + WstTags.RequestSecurityToken.TagName)
                                   .Descendants(WstTags.Claims.Ns + WstTags.Claims.TagName)
                                   .Descendants()
                                   .Where(x => x.Attribute("Uri").Value.Equals("medcom:ITSystemName"))
                                   .Elements()
                                   .FirstOrDefault()
                                   ;
            itSystemClaim.Value = "TAMPERED";
            var request = requestModelBuilder.Build(requestDocument);

            //THEN
            try
            {
                request.ValidateSignature();
                Assert.Fail("signature validation should fail");
            }
            catch (Exception e)
            {
                Assert.True(e.Message.Contains("signature could not be validated"), e.Message);
            }
        }


        [Test]
        public void TestMissingAssertion()
        {
            AbstractOIOBSTSAMLAssertionBuilder<OIOH2BSTSAMLAssertion> assertionBuilder = null;
            requestBuilder.SetOIOSAMLAssertion(assertionBuilder);
            try
            {
                requestBuilder.Build();
                Assert.Fail("Build should fail");
            }
            catch (ModelException e)
            {
                Assert.True(e.Message.Contains("OIOBSTSAMLAssertion is mandatory - but was null"), e.Message);
            }
        }

        [Test]
        public void testMissingAudience()
        {
            requestBuilder.Audience = null;
            try
            {
                requestBuilder.Build();
                Assert.Fail("Build should fail");
            }
            catch (ModelException e)
            {
                Assert.True(e.Message.Contains("audience is mandatory - but was null"), e.Message);
            }
        }

        [Test]
        public void TestMissingSigningVault()
        {
            requestBuilder.SigningVault = null;
            try
            {
                requestBuilder.Build();
                Assert.Fail("Build should fail");
            }
            catch (ModelException e)
            {
                Assert.True(e.Message.Contains("signingVault is mandatory - but was null"), e.Message);
            }
        }

        [Test]
        public void TestMissingITSystemName()
        {
            requestBuilder.ItSystemName = null;
            try
            {
                requestBuilder.Build();
                Assert.Fail("Build should fail");
            }
            catch (ModelException e)
            {
                Assert.True(e.Message.Contains("ITSystemName is mandatory - but was null"), e.Message);
            }
        }

        private void Validate(OIOBSTSAMLAssertion assertion)
        {
            Assert.AreEqual(tokenType, assertion.AssertionType, "Uventet type");
            Assert.AreEqual(issuer, assertion.Issuer, "Uventet Issuer");
            Assert.AreEqual("KorsbaekKommune\\MSK", assertion.SubjectNameId, "Uventet Name ID");
            Assert.AreEqual(holderOfKeyVault.GetSystemCredentials(), assertion.GetHolderOfKeyCertificate(), "Uventet Holder Of Key Certificate");
            Assert.AreEqual(audience, assertion.AudienceRestriction, "Uventet Audience");
            //Tjekker tid uden MS
            var notOnOrAfterOffset = TimeZoneInfo.Local.IsDaylightSavingTime(assertion.NotOnOrAfter) ? 2 : 1;
            Assert.AreEqual(validTo.AddHours(notOnOrAfterOffset).ToString(), assertion.NotOnOrAfter.ToString(), "Uventet NotOnOrAfter date"); 
            Assert.NotNull(assertion.NotOnOrAfter);
            Assert.AreEqual(assertionSigningVault.GetSystemCredentials(), assertion.SigningCertificate, "Uventet Signing Certificate");
            Assert.AreEqual(assuranceLevel, assertion.AssuranceLevel, "Uventet Assurance Level");
            if (tokenType != OIOBSTSAMLAssertion.Type.OIOH3BST)
            {
                Assert.AreEqual(cpr, assertion.Cpr, "Uventet CPR");
            }
            Assert.Null(assertion.RidNumberIdentifier, "Forventede ikke RID i Assertion");
            Assert.AreEqual(cvr, assertion.CvrNumberIdentifier, "Uventet CVR");
            Assert.AreEqual(organizationName, assertion.OrganizationName, "Uventet Organization Name");

            //Test getMetoder der ikke er sat i GIVEN
            string expectedNameIdFormat = tokenType == OIOBSTSAMLAssertion.Type.OIOH2BST ? "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"
                    : "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
            Assert.AreEqual(expectedNameIdFormat, assertion.SubjectNameIdFormat, "Uventet Name ID format");
            Assert.NotNull(assertion.Dom, "Forventede DOM objekt fra Assertion");
            Assert.NotNull(assertion.Id, "Forventede ID fra Assertion");
            string expectedVersion = tokenType == OIOBSTSAMLAssertion.Type.OIOH2BST ? "DK-SAML-2.0" : "OIO-SAML-3.0";
            Assert.AreEqual(expectedVersion, assertion.SpecVersion, "Uventet Spec version");
            Assert.AreEqual(uuid, assertion.Uid, "Uventet UUID fra Assertion");

            if (tokenType == OIOBSTSAMLAssertion.Type.OIO3BST)
            {
                Assert.Null(assertion.BasicPrivileges, "Forventede ikke Privileges fra Assertion");
            }
            else
            {
                Assert.NotNull(assertion.BasicPrivileges, "Forventede Privileges fra Assertion");
                Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3", assertion.BasicPrivileges.Privileges["urn:dk:gov:saml:cvrNumberIdentifier:20301823"].FirstOrDefault(), "Uventet Privilege");
            }

            //Test getAttribute
            Assert.Null(assertion.GetAttributeValue("NonExistingAttribute"), "Uventet resultat af NonExistingAttribute");
            Assert.NotNull("Uventet resultat af OIOH2BSTSAMLAssertion.Attribute.CVR", assertion.GetAttributeValue(OIOH2BSTSAMLAssertion.SamlCvr.Name));
        }

        private OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> requestBuilderFromAssertionBuilder(ICredentialVault signingVault, AbstractOIOBSTSAMLAssertionBuilder<OIOH2BSTSAMLAssertion> assertionBuilder)
        {
            OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> requestBuilder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionToIDCardRequestDOMBuilder();
            requestBuilder.SetOIOSAMLAssertion(assertionBuilder);
            requestBuilder.Audience = "https://fmk";
            requestBuilder.ItSystemName = "Korsbæk Kommunes IT systemer";
            requestBuilder.SigningVault = signingVault;
            requestBuilder.UserRole = userRole;
            requestBuilder.UserAuthorizationCode = authorizationCode;
            requestBuilder.SubjectNameId = "Mads_Skjern";

            return requestBuilder;
        }

        private OIOH2BSTSAMLAssertionBuilder CreateOIOH2BSTSAMLAssertionBuilder(ICredentialVault holderOfKeyVault, ICredentialVault signingVault)
        {
            OIOH2BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionBuilder();
            issuer = "https://oioh2bst-issuer.dk";
            builder.Issuer = issuer;
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            builder.Audience = audience;
            builder.NotOnOrAfter = validTo;
            builder.SigningVault = signingVault;
            assuranceLevel = "4";
            builder.SetAssuranceLevel("NIST", assuranceLevel);
            builder.Uuid = uuid;
            builder.Cvr = cvr; 
            builder.OrganizationName = organizationName;
            builder.Cpr = cpr;
            BasicPrivilegesDOMBuilder privilegesBuilder = new();
            privilegesBuilder.AddPrivilege("urn:dk:gov:saml:cvrNumberIdentifier:20301823", "urn:dk:healthcare:national-federation-role:code:41003:value:PlejeAssR3");
            builder.Privileges = privilegesBuilder;

            return builder;
        }
    }
}
