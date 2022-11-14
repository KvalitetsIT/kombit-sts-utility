using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using NUnit.Framework;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateSuiteOces2), "1802602810", "ZXCVB")]
    [TestFixture(typeof(CertificateSuiteOces3), "0306894781", "KT2Z4")]
    public class FederationTest<CERTIFICATE_SUITE> : AbstractTest where CERTIFICATE_SUITE : ICertificateSuite, new()
    {
        private CERTIFICATE_SUITE certificateSuite;
        private readonly string mocesCpr;
        private readonly string authorization;

        public FederationTest(string mocesCpr, string authorization)
        {
            certificateSuite = new CERTIFICATE_SUITE();
            this.mocesCpr = mocesCpr;
            this.authorization = authorization;
        }

        [Test]
        public void IsTrustedStsCertificateTest()
        {
            SOSIFactory factory = CreateSOSIFactoryWithTestFederation(certificateSuite.MocesCprValid.Certificate);

            var idCard = CreateIdCardForSTS(certificateSuite.MocesCprValid, mocesCpr, authorization);

            idCard.Sign<Assertion>(factory.SignatureProvider);

            var idc = SealUtilities.SignIn(idCard, "NETS DANID A/S", TestConstants.SecurityTokenService);

            Assert.DoesNotThrow(() => idc.InternalValidateSignature(factory.Federation, true));
        }

        [Test]
        public void SelfSignedIdCardTest()
        {
            var factory = CreateSOSIFactoryWithTestFederation(certificateSuite.MocesCprValid.Certificate);

            var idCard = CreateIdCardForSTS(certificateSuite.MocesCprValid, mocesCpr, authorization);

            idCard.Sign<Assertion>(factory.SignatureProvider);

            Assert.Throws<ModelException>(() => idCard.InternalValidateSignature(factory.Federation, true));
        }

        [Test]
        public void SimpleMocesChainTest()
        {
            SOSIFactory factory = CreateSOSIFactoryWithTestFederation(certificateSuite.MocesCprValid.Certificate);
            bool validation = factory.Federation.IsValidCertificate(certificateSuite.MocesCprValid.Certificate);

            Assert.True(validation);
        }

        [Test]
        public void SimpleFocesChainTest()
        {
            SOSIFactory factory = CreateSOSIFactoryWithTestFederation(certificateSuite.FocesValid.Certificate);
            bool validation = factory.Federation.IsValidCertificate(certificateSuite.FocesValid.Certificate);

            Assert.True(validation);
        }


        [Test]
        public void ExpiredCertificateTest()
        {
            if(certificateSuite is CertificateSuiteOces3)
            {
                Assert.Inconclusive("Unable to test expired OCES3 certificates until we have a certificate that is expired."); // TODO Det er ikke muligt at teste udløbne OCES3 certifikater før vi har et certifikat der er udløbet.
            }
            X509Certificate2 newCert = certificateSuite.FocesExpired.Certificate;

            SOSIFactory factory = CreateSOSIFactoryWithSosiFederation(certificateSuite.MocesCprValid.Certificate);
            bool validation = factory.Federation.IsValidCertificate(newCert);

            Assert.False(validation);
        }

        [Test]
        public void RevokedCertificateTest()
        {
            X509Certificate2 newCert = new(certificateSuite.MocesRevoked.Certificate);

            SOSIFactory factory = CreateSOSIFactoryWithTestFederation(certificateSuite.MocesCprValid.Certificate);
            bool validation = factory.Federation.IsValidCertificate(newCert);

            Assert.False(validation);
        }

        [Test]
        public void InvalidChainSTestFederationTest()
        {
			X509Certificate2 newCert = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources", "SelfSigned.pfx"), "Test1234");

            SOSIFactory factory = CreateSOSIFactoryWithTestFederation(certificateSuite.MocesCprValid.Certificate);

			Assert.False(factory.Federation.IsValidCertificate(newCert));
        }

		[Test]
        public void InvalidChainSosiFederationTest()
        {
            X509Certificate2 newCert = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","SelfSigned.pfx"), "Test1234");

            SOSIFactory factory = CreateSOSIFactoryWithSosiFederation(certificateSuite.MocesCprValid.Certificate);

            Assert.False(factory.Federation.IsValidCertificate(newCert));
        }

        [Test]
        public void SosiFederationTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactoryWithSosiFederation(certificateSuite.MocesCprValid.Certificate);

            //Create IdCard
            UserIdCard idCard = CreateIdCardForSTS(certificateSuite.MocesCprValid, mocesCpr, authorization);

            //Sign IdCard
            idCard.Sign<Assertion>(factory.SignatureProvider);

            UserIdCard idc = (UserIdCard)SealUtilities.SignIn(idCard, "NETS DANID A/S", TestConstants.SecurityTokenService);

            //Assert that STS certificate fails due to mismatch in prefix/cvr
            Assert.Throws<ModelException>(delegate { idc.InternalValidateSignature(factory.Federation, true); });
        }
    }
}
