using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using dk.nsi.seal;
using dk.nsi.seal.Model;
using NUnit.Framework;
using SealTest.Certificate;

namespace SealTest.Model
{
    /// <summary>
    /// Tests the SignatureUtilClass
    /// Must be run from an Nets whitelisted IP or the CRL check will fail
    /// </summary>
    [TestFixture(typeof(CertificateSuiteOces2))]
    [TestFixture(typeof(CertificateSuiteOces3))]
    public class SignatureUtilTest<TCertSuite> where TCertSuite : ICertificateSuite, new()
    {
        private readonly TCertSuite suite;
        private readonly X509Certificate2 focesValid;
        private readonly X509Certificate2 vocesValid;

        public SignatureUtilTest()
        {
            suite = new TCertSuite();
            focesValid = suite.FocesValid.Certificate;
            vocesValid = suite.VocesValid.Certificate;
        }

        [Test]
        public void TestSignAndValidateWithTrustWithRevoked()
        {
            //Add test certificate to vault
            Assert.True(SignAndValidate(suite.FocesValid, true, true, false));
        }

        [Test]
        public void TestSignAndValidateWithTrustWithoutRevoked()
        {
            //Add test certificate to vault
            var newCert = focesValid;
            newCert.Verify();
            Assert.True(SignAndValidate(suite.FocesValid, true, false, false));
        }

        [Test]
        public void TestSignAndValidateWithoutTrustWithoutRevoked()
        {
            //Add test certificate to vault
            var result = SignAndValidate(suite.FocesValid, false, false, false);
            Assert.True(result);
        }

        [Test]
        public void TestSignAndValidateFailDate()
        {
            if (suite is CertificateSuiteOces3)
            {
                Assert.Inconclusive(
                    "Unable to test expired OCES3 certificates until we have a certificate that is expired."); // TODO Det er ikke muligt at teste udløbne OCES3 certifikater før vi har et certifikat der er udløbet.
            }

            var result = SignAndValidate(suite.FocesExpired, true, true, true);
            Assert.False(result);
        }

        [Test]
        public void TestSignAndValidateFailRevoked()
        {
            //Add test certificate to vault
            try
            {
                SignAndValidate(suite.FocesRevoked, true, true, false);
                Assert.Fail("It must not be possible to sign and validate revoked certificate.");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<ModelException>(e);
            }
        }


        private bool SignAndValidate(ICertificate cert, bool checkTrust, bool checkRevoked, bool checkDate) =>
            SignAndValidate(cert.Certificate, checkTrust, checkRevoked, checkDate, cert.Cvr);

        private bool SignAndValidate(X509Certificate2 cert, bool checkTrust, bool checkRevoked, bool checkDate,
            string cvr)
        {
            var ass = AssertionMaker.MakeAssertionForSTS(cert, 4, cvr);

            var signedAss = SealUtilities.SignAssertion(ass, cert);
            var signedXml = Serialize(signedAss);

            return SignatureUtil.Validate(signedXml.Root, checkTrust, checkRevoked, checkDate);
        }


        [Test]
        public void TestSignAndValidateNotTrusted()
        {
            //Add test certificate to vault
            var newCert = focesValid;
            var cert2 = vocesValid;

            var ass = AssertionMaker.MakeAssertionForSTS(suite.FocesValid, 4);

            var signedAss = SealUtilities.SignAssertion(ass, newCert);
            var signedXml = Serialize(signedAss);

            try
            {
                SignatureUtil.Validate(signedXml.Root, true, true, false);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<ModelException>(e);
            }
        }

        [Test]
        public void TestSignAndValidateSelfSignedWithTrustWithCrl()
        {
            var newCert =
                new X509Certificate2(
                    Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources",
                        "SelfSigned.pfx"), "Test1234");
            Assert.True(SignAndValidate(newCert, true, true, false, "CVR"));
        }

        [Test]
        public void TestSignAndValidateSelfSignedWithTrustWithoutCrl()
        {
            var newCert =
                new X509Certificate2(
                    Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources",
                        "SelfSigned.pfx"), "Test1234");
            Assert.True(SignAndValidate(newCert, true, false, false, "CVR"));
        }


        private static XDocument Serialize<T>(T element) =>
            XDocument.Load(Serialize2Stream(element), LoadOptions.PreserveWhitespace);

        private static Stream Serialize2Stream<T>(T element)
        {
            var ms = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings { Indent = false }))
            {
                GetSerializer<T>().Serialize(xmlWriter, element);
            }

            ms.Position = 0;
            return ms;
        }

        private static XmlSerializer GetSerializer<T>()
        {
            var t = typeof(T);
            var rootns = t.GetCustomAttributes(false).OfType<XmlTypeAttribute>().FirstOrDefault().Namespace;

            return new XmlSerializer(t, rootns);
        }
    }
}