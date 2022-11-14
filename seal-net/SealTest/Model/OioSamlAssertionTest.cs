using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.Constants;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SealTest.Model
{
    class OioSamlAssertionTest : AbstractTest
    {
        [Test]
        public void TestConstructionFromOioSamlSample()
        {

            var assertion = new OioSamlAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                                               "/Resources/oiosaml-examples/test-oiosamljava-authentication-assertion.xml"));

            Assert.AreEqual("pfx426233b1-9ce1-99cd-c755-19988e670e46", assertion.Id);

            Assert.AreEqual("http://fmkwebtest.trifork.netic.dk/idp/saml2/idp/metadata.php", assertion.Issuer);

            Assert.AreEqual(DateTime.Parse("2012-09-20T10:56:24Z"), assertion.NotBefore);

            Assert.AreEqual(DateTime.Parse("2012-09-20T11:01:54Z"), assertion.NotOnOrAfter);

            Assert.AreEqual("2", assertion.AssuranceLevel);

            Assert.AreEqual("Terri Dalsgård", assertion.CommonName);

            Assert.AreEqual("Dalsgård", assertion.SurName);

            Assert.AreEqual("0101584162", assertion.Cpr);

            Assert.AreEqual("certifikat@tdc.dk", assertion.Email);

            Assert.AreEqual("25767535", assertion.CvrNumberIdentifier);

            Assert.AreEqual("TDC TOTALLØSNINGER A/S", assertion.OrganizationName);

            Assert.AreEqual("1118061020235", assertion.RidNumberIdentifier);

            Assert.AreEqual("http://saml.vronding/fmk-gui", assertion.AudienceRestriction);

            Assert.AreEqual(DateTime.Parse("2012-09-20T10:56:54Z"), assertion.UserAuthenticationInstant);

            Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion);

            Assert.AreEqual("CVR:25767535-RID:1118061020234", assertion.SubjectNameId);

            Assert.AreEqual("urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName", assertion.SubjectNameIdFormat);

            Assert.AreEqual("http://vronding1:8080/fmk/saml/SAMLAssertionConsumer", assertion.Recipient);
            X509Certificate userCertificate = assertion.UserCertificate;

            Assert.IsNotNull(userCertificate);

            Assert.AreEqual(
                new X500DistinguishedName(
                    "CN=Test Bruger 1 + SERIALNUMBER=CVR:25767535-RID:1118061020232, O=TDC TOTALLØSNINGER A/S // CVR:25767535, C=DK").Name,
                new X500DistinguishedName(userCertificate.Subject).Name);
            try
            {
                assertion.ValidateTimestamp();
                Assert.Fail();
            }
            catch (ModelException e)
            {
                Assert.IsTrue(e.Message.StartsWith("OIOSAML token no longer valid"));
            }
        }

        [Test]
        public void TestConstructionFromNewNemLoginSampleOne()
        {

            var assertion = new OioSamlAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                                               "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-1.xml"));

            Assert.AreEqual("_30c5ecce-9108-4df2-bee2-2d1358973444", assertion.Id);

            Assert.AreEqual("https://saml.test-nemlog-in.dk/", assertion.Issuer);

            Assert.AreEqual(DateTime.Parse("2012-07-03T09:40:55.963Z"), assertion.NotBefore);

            Assert.AreEqual(DateTime.Parse("2012-07-03T10:40:55.963Z"), assertion.NotOnOrAfter);

            Assert.AreEqual("3", assertion.AssuranceLevel);

            Assert.AreEqual("Søren Test Mors", assertion.CommonName);

            Assert.AreEqual("", assertion.SurName);

            Assert.IsNull(assertion.Cpr);

            Assert.AreEqual("soren@signaturgruppen.dk", assertion.Email);

            Assert.AreEqual("29915938", assertion.CvrNumberIdentifier);

            Assert.AreEqual("SIGNATURGRUPPEN A/S // CVR:29915938", assertion.OrganizationName);

            Assert.AreEqual("soren", assertion.RidNumberIdentifier);

            Assert.AreEqual("https://saml.remote.signaturgruppen.dk", assertion.AudienceRestriction);

            Assert.AreEqual(DateTime.Parse("2012-07-03T09:40:46.104Z"), assertion.UserAuthenticationInstant);

            Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion);

            Assert.AreEqual("C=DK,O=SIGNATURGRUPPEN A/S // CVR:29915938,CN=Søren Test Mors,Serial=CVR:29915938-RID:soren",
                assertion.SubjectNameId);

            Assert.AreEqual("urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName", assertion.SubjectNameIdFormat);

            Assert.AreEqual("https://remote.signaturgruppen.dk/nemlogin/unsecure/logon.ashx", assertion.Recipient);

            Assert.AreEqual("CN=TDC OCES Systemtest CA II, O=TDC, C=DK", assertion.CertificateIssuer);
            X509Certificate userCertificate = assertion.UserCertificate;

            Assert.IsNotNull(userCertificate);

            Assert.AreEqual(
                new X500DistinguishedName(
                    "CN=Søren Test Mors + SERIALNUMBER=CVR:29915938-RID:soren, O=SIGNATURGRUPPEN A/S // CVR:29915938, C=DK").Name,
                new X500DistinguishedName(userCertificate.Subject).Name);
            try
            {
                assertion.ValidateTimestamp();
                Assert.Fail();
            }
            catch (ModelException e)
            {

                Assert.IsTrue(e.Message.StartsWith("OIOSAML token no longer valid"));
            }
        }

        [Test]
        public void TestConstructionFromNewNemLoginSampleTwo()
        {

            var assertion = new OioSamlAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                                               "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-2.xml"));

            Assert.AreEqual("_5a49e560-5312-4237-8f32-2ed2b58cfcf7", assertion.Id);

            Assert.AreEqual("https://saml.test-nemlog-in.dk/", assertion.Issuer);

            Assert.AreEqual(DateTime.Parse("2012-09-27T08:51:13.884Z"), assertion.NotBefore);

            Assert.AreEqual(DateTime.Parse("2012-09-27T09:51:13.884Z"), assertion.NotOnOrAfter);

            Assert.AreEqual("3", assertion.AssuranceLevel);

            Assert.AreEqual("Amaja Christiansen", assertion.CommonName);

            Assert.AreEqual("", assertion.SurName);

            Assert.AreEqual("2408631478", assertion.Cpr);

            Assert.AreEqual("jso@trifork.com", assertion.Email);

            Assert.AreEqual("25520041", assertion.CvrNumberIdentifier);

            Assert.AreEqual("TRIFORK SERVICES A/S // CVR:25520041", assertion.OrganizationName);

            Assert.AreEqual("42041556", assertion.RidNumberIdentifier);

            Assert.AreEqual("https://saml.fmk.staging.fmk-online.dk", assertion.AudienceRestriction);

            Assert.AreEqual(DateTime.Parse("2012-09-27T08:50:38.681Z"), assertion.UserAuthenticationInstant);

            Assert.AreEqual("DK-SAML-2.0", assertion.SpecVersion);

            Assert.AreEqual(
                "C=DK,O=TRIFORK SERVICES A/S // CVR:25520041,CN=Amaja Christiansen,Serial=CVR:25520041-RID:42041556",
                assertion.SubjectNameId);

            Assert.AreEqual("urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName", assertion.SubjectNameIdFormat);

            Assert.AreEqual("CN=TDC OCES Systemtest CA II, O=TDC, C=DK", assertion.CertificateIssuer);
            X509Certificate userCertificate = assertion.UserCertificate;

            Assert.IsNotNull(userCertificate);

            Assert.AreEqual(
                new X500DistinguishedName(
                    "CN=Amaja Christiansen + SERIALNUMBER=CVR:25520041-RID:42041556, O=TRIFORK SERVICES A/S // CVR:25520041, C=DK").Name,
                new X500DistinguishedName(userCertificate.Subject).Name);

            Assert.AreEqual("https://staging.fmk-online.dk/fmk/saml/SAMLAssertionConsumer", assertion.Recipient);
            assertion.ValidateSignatureAndTrust();
            try
            {
                assertion.ValidateTimestamp();
                Assert.Fail();
            }
            catch (ModelException e)
            {
                Assert.IsTrue(e.Message.StartsWith("OIOSAML token no longer valid"));
            }
        }


        [Test]
        public void TestNullDocument()
        {
            Assert.Throws<ArgumentException>(() => new OioSamlAssertion((XElement)null));
        }

        [Test]
        public void TestInvalidSample()
        {
            var assertion = new OioSamlAssertion(XElement.Load(NUnit.Framework.TestContext.CurrentContext.TestDirectory +
                                                               "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-1.xml"));

            var issuer = assertion.XAssertion.Descendants(SamlTags.Issuer.Ns + SamlTags.Issuer.TagName).FirstOrDefault();
            issuer.Remove();
            new OioSamlAssertion(assertion.XAssertion);
        }

        [Test]
        public void TestInvalidSamlFragment()
        {
            var document = XDocument.Parse("<saml:Issuer xmlns:saml='urn:oasis:names:tc:SAML:2.0:assertion'>foo</saml:Issuer>");
            var ex = Assert.Throws<ArgumentException>(() => new OioSamlAssertion(document.Root));
            Assert.AreEqual("Element is not a SAML assertion", ex.Message);
        }

        [Test]
        public void TestNonSamlFragment()
        {
            var document = XDocument.Parse("<foo/>");
            var ex = Assert.Throws<ArgumentException>(() => new OioSamlAssertion(document.Root));
            Assert.AreEqual("Element is not a SAML assertion", ex.Message);
        }

        [Test]
        public void TestUnsignedAssertion()
        {
            var xElement = XElement.Load(TestContext.CurrentContext.TestDirectory +
                             "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-2.xml");
            var signature = xElement.Descendants(DsTags.Signature.Ns + DsTags.Signature.TagName).FirstOrDefault();
            signature.Remove();
            var assertion = new OioSamlAssertion(xElement);
            var ex =
                Assert.Throws<ModelException>(
                    () => assertion.ValidateSignatureAndTrust());
            Assert.AreEqual("OIOSAMLAssertion is not signed", ex.Message);
        }

        [Test]
        public void TestBrokenSignature()
        {

            var xElement = XElement.Load(TestContext.CurrentContext.TestDirectory +
                                         "/Resources/oiosaml-examples/test-new-nemlogin-authentication-assertion-2.xml",
                LoadOptions.None);
            var assertion = new OioSamlAssertion(xElement);
            assertion.ValidateSignatureAndTrust();

            var attributes = xElement.Descendants(SamlTags.Attribute.Ns + SamlTags.Attribute.TagName);
            var nameNode =
                attributes.FirstOrDefault(
                    element => element.Attribute(SamlAttributes.Name).Value.Equals(OioSamlAttributes.CommonName));
            nameNode.Value = "Ronnie Romkugle";
            assertion = new OioSamlAssertion(xElement);
            try
            {
                assertion.ValidateSignatureAndTrust();
            }
            catch (ModelException e)
            {
                Assert.AreEqual("Signature on OIOSAMLAssertion is invalid", e.Message);
            }
        }
    }
}
