using System;
using System.Xml.Linq;
using dk.nsi.seal.Model;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SealTest.Model
{
    class OioSaml3AssertionTest : AbstractTest
    {
        [Test]
        public void TestOIOH2BST()
        {
            var assertion = new OIOH2BSTSAMLAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                                   "/Resources/oiosaml-examples/OIOSAML3Assertion/OIOH2BST-example.xml"));
            //OIOH2BSTSAMLAssertion assertion = new OIOH2BSTSAMLAssertion(OIOH2BST.getDocumentElement());
            Assert.AreEqual("3", assertion.AssuranceLevel);
            Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction);
            //assertNull("Forventede ikke Certificate Issuer på OIOSAML2Assertion", assertion.getCertificateIssuer());	
            //assertNull("Forventede ikke Certificate Serial på OIOSAML2Assertion", assertion.getCertificateSerial());	
            //assertNull(assertion.getCommonName()); //dk.sosi.seal.model.ModelException: Mandatory
            Assert.AreEqual("1802602810", assertion.Cpr);
            Assert.AreEqual("20301823", assertion.CvrNumberIdentifier);
            //Assert.AreEqual("", assertion.getDeliveryNotOnOrAfter()); //java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            //Assert.AreEqual("", assertion.getEmail()); //dk.sosi.seal.model.ModelException: Mandatory 'email' SAML attribute (urn:oid:0.9.2342.19200300.100.1.3) is missing	
            Assert.AreEqual("_f3070cce-b0ce-4025-b374-ada158cb137c", assertion.Id);
            Assert.AreEqual("https://seblogin.nsi.dk/runtime/", assertion.Issuer);
            //Assert.AreEqual("", assertion.getNotBefore());	//java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            Assert.AreEqual(DateTime.Parse("2020-11-13T12:22:50.027Z"), assertion.NotOnOrAfter);
            Assert.AreEqual("Korsbæk Kommune", assertion.OrganizationName);
            //Assert.AreEqual("", assertion.getRecipient());	
            Assert.AreEqual("1231593107593", assertion.RidNumberIdentifier);
            //Assert.AreEqual(SOSITestUtils.NEW_NEMLOGIN_IDP_CERTIFICATE, assertion.getSigningCertificate()); // TODO Signing certificate
            Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion);
            Assert.AreEqual("KorsbaekKommune\\MSK", assertion.SubjectNameId);
            Assert.AreEqual("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified", assertion.SubjectNameIdFormat);
            //Assert.AreEqual("", assertion.getSurName()); //Fejler da den forventes at være mandatory ift. eksisterende OIOSAMLAssertion
            //assertNull("Forventede ikke UID på OIOSAML2Assertion", assertion.getUID());
            //Assert.AreEqual("", assertion.getUserAuthenticationInstant()); //dk.sosi.seal.model.ModelException: Date element cannot be null
            //assertNull("Forventede ikke User Certificate på OIOSAML2Assertion", assertion.getUserCertificate());
            //assertNull("Forventede ikke User ID Card på OIOSAML2Assertion", assertion.getUserIDCard());
        }

        [Test]
        public void TestOIOH3BST()
        {
            //OIOH3BSTSAMLAssertion assertion = new OIOH3BSTSAMLAssertion(OIOH3BST.getDocumentElement());
            var assertion = new OIOH3BSTSAMLAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                       "/Resources/oiosaml-examples/OIOSAML3Assertion/OIOH3BST-example.xml"));
            /* Required */
            ValidateOIOH3BSTAssertion(assertion);
        }

        public static void ValidateOIOH3BSTAssertion(OIOH3BSTSAMLAssertion assertion)
        {

            Assert.AreEqual("Substantial", assertion.AssuranceLevel);
            Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction);
            //assertNull("Forventede ikke Certificate Issuer på OIOSAML3Assertion", assertion.getCertificateIssuer());	
            //assertNull("Forventede ikke Certificate Serial på OIOSAML3Assertion", assertion.getCertificateSerial());	
            //assertNull(assertion.getCommonName()); //dk.sosi.seal.model.ModelException: Mandatory
            Assert.Null(assertion.Cpr);
            Assert.AreEqual("20301823", assertion.CvrNumberIdentifier);
            //Assert.AreEqual("", assertion.getDeliveryNotOnOrAfter()); //java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            //Assert.AreEqual("", assertion.getEmail());	//dk.sosi.seal.model.ModelException: Mandatory 'email' SAML attribute (urn:oid:0.9.2342.19200300.100.1.3) is missing	
            Assert.AreEqual("_f3070cce-b0ce-4025-b374-ada158cb137c", assertion.Id);
            Assert.AreEqual("https://idp.korsbaek-kommune.dk", assertion.Issuer);
            //Assert.AreEqual("", assertion.getNotBefore());	//java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            Assert.AreEqual(DateTime.Parse("2020-11-13T12:22:50.027Z"), assertion.NotOnOrAfter);
            Assert.AreEqual("Korsbæk Kommune", assertion.OrganizationName);
            //Assert.AreEqual("", assertion.getRecipient());	
            Assert.Null(assertion.RidNumberIdentifier);
            //Assert.AreEqual(SOSITestUtils.NEW_NEMLOGIN_IDP_CERTIFICATE, assertion.getSigningCertificate()); // TODO
            Assert.AreEqual("OIO-SAML-3.0", assertion.SpecVersion);
            Assert.AreEqual("OIO-SAML-H-3.0", assertion.SpecVersionAdditional);
            Assert.AreEqual("KorsbaekKommune\\MSK", assertion.SubjectNameId);
            Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", assertion.SubjectNameIdFormat);
            //Assert.AreEqual("", assertion.getSurName()); //Fejler da den forventes at være mandatory ift. eksisterende OIOSAMLAssertion
            //assertNull("Forventede ikke UID på OIOSAML3Assertion", assertion.getUID());
            //Assert.AreEqual("", assertion.getUserAuthenticationInstant()); //dk.sosi.seal.model.ModelException: Date element cannot be null
            //assertNull("Forventede ikke User Certificate på OIOSAML3Assertion", assertion.getUserCertificate());
            //assertNull("Forventede ikke User ID Card på OIOSAML3Assertion", assertion.getUserIDCard());
        }

        [Test]
        public void TestOIO3BST()
        {
            //OIO3BSTSAMLAssertion assertion = new OIO3BSTSAMLAssertion(OIO3BST.getDocumentElement());
            var assertion = new OIO3BSTSAMLAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                           "/Resources/oiosaml-examples/OIOSAML3Assertion/OIO3BST-unencrypted-example.xml"));
            /* Required */
            Assert.AreEqual("Substantial", assertion.AssuranceLevel);
            Assert.AreEqual("https://sts.sosi.dk/", assertion.AudienceRestriction);
            //assertNull("Forventede ikke Certificate Issuer på OIOSAML3Assertion", assertion.getCertificateIssuer());	
            //assertNull("Forventede ikke Certificate Serial på OIOSAML3Assertion", assertion.getCertificateSerial());	
            //assertNull(assertion.getCommonName()); //dk.sosi.seal.model.ModelException: Mandatory
            Assert.Null(assertion.Cpr);
            Assert.AreEqual("20301823", assertion.CvrNumberIdentifier);
            //Assert.AreEqual("", assertion.getDeliveryNotOnOrAfter()); //java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            //Assert.AreEqual("", assertion.getEmail());	//dk.sosi.seal.model.ModelException: Mandatory 'email' SAML attribute (urn:oid:0.9.2342.19200300.100.1.3) is missing	
            Assert.AreEqual("_f3070cce-b0ce-4025-b374-ada158cb137c", assertion.Id);
            //Assert.AreEqual("https://idp.korsbaek-kommune.dk", assertion.getIssuer());
            //Assert.AreEqual("", assertion.getNotBefore());	//java.lang.IllegalArgumentException: DateTimeString cannot be null or empty
            Assert.AreEqual(DateTime.Parse("2020-11-13T12:22:50.027Z"), assertion.NotOnOrAfter);
            Assert.AreEqual("Korsbæk Kommune", assertion.OrganizationName);
            //Assert.AreEqual("", assertion.getRecipient());	
            Assert.Null(assertion.RidNumberIdentifier);
            //Assert.AreEqual(SOSITestUtils.NEW_NEMLOGIN_IDP_CERTIFICATE, assertion.getSigningCertificate()); // TODO 
            Assert.AreEqual("OIO-SAML-3.0", assertion.SpecVersion);
            Assert.AreEqual(" https://data.gov.dk/model/core/eid/professional/uuid/323e4567-e89b-12d3-a456-426655440000", assertion.SubjectNameId);
            Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", assertion.SubjectNameIdFormat);
            //Assert.AreEqual("", assertion.getSurName()); //Fejler da den forventes at være mandatory ift. eksisterende OIOSAMLAssertion
            //assertNull("Forventede ikke UID på OIOSAML3Assertion", assertion.getUID());
            //Assert.AreEqual("", assertion.getUserAuthenticationInstant()); //dk.sosi.seal.model.ModelException: Date element cannot be null
            //assertNull("Forventede ikke User Certificate på OIOSAML3Assertion", assertion.getUserCertificate());
            //assertNull("Forventede ikke User ID Card på OIOSAML3Assertion", assertion.getUserIDCard());
        }
    }
}
