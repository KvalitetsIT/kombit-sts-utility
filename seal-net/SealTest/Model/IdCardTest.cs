using System;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using NUnit.Framework;
using System.IO;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateSuiteOces2))]
    [TestFixture(typeof(CertificateSuiteOces3))]
    public class IdCardTest<CERTIFICATE> : AbstractTest where CERTIFICATE : ICertificateSuite, new()
    {
        private ICertificateSuite CertificateSuite;

        public IdCardTest()
        {
            this.CertificateSuite = new CERTIFICATE();
        }

        [Test]
        public void IdCardValidatorTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard with missing UserGivenName
            UserIdCard idCard = SOSIFactory.CreateNewUserIdCard("ItSystem", new UserInfo("12345678", null, "Person", "test@person.dk", "Tester", "Læge", "12345"), new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.MocesTrustedUser, "", "", factory.GetCertificate(), "alt");

            //Try to sign the idCard
            Assert.Throws<ModelException>(delegate { idCard.Sign<Assertion>(factory.SignatureProvider); });
        }

        [Test]
        public void IdCardNullUserInfoTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard with missing UserInfo
            Assert.Throws<ModelException>(() => 
            SOSIFactory.CreateNewUserIdCard("ItSystem", null, new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.MocesTrustedUser, "", "", factory.GetCertificate(), "alt"));
        }

        [Test]
        public void IdCardNullSystemInfoTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate); 

            //Create IdCard with missing UserInfo
            Assert.Throws<ModelException>(
                () =>  SOSIFactory.CreateNewSystemIdCard("", new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.MocesTrustedUser, null, null, factory.GetCertificate(), "alt"));
        }

        [Test]
        public void IdCardNullCareProviderTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate); 

            //Create IdCard with missing UserInfo
            Assert.Throws<ModelException>(
                () => SOSIFactory.CreateNewSystemIdCard("ItSystem", null, AuthenticationLevel.MocesTrustedUser, null, null, factory.GetCertificate(), "alt"));
        }

        [Test]
        public void IdCardUserNamePassTest() 
        {
            var factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard with username/password
            var idCard = CreateUserIdCard(factory, "user","test123");

            //Get Assertion
            var ass = idCard.GetAssertion<Assertion>();

            Assert.True(ass.Subject.SubjectConfirmation.SubjectConfirmationData.Item.GetType() == typeof(UsernameToken));

            //Assert assertion was created succesfully
            Assert.NotNull(ass);
            Assert.NotNull(idCard.Xassertion);
        }

        [Test]
        public void IdCardMocesSignTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard
            UserIdCard idCard = CreateMocesUserIdCard();

            //Sign IdCard
            Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);
            Assertion ass2 = idCard.GetAssertion<Assertion>();

            //Assert assertion was created succesfully
            Assert.NotNull(ass);
            Assert.NotNull(idCard.Xassertion);

            //Make sure the assertion returned from Sign and Get are the same.
            Assert.True(ass.Signature.SignatureValue.ToString() == ass2.Signature.SignatureValue.ToString());
        }

        [Test]
        public void ValidateMocesSignatureTest()
        {
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate); 
            UserIdCard idCard = CreateMocesUserIdCard();
            idCard.Sign<Assertion>(factory.SignatureProvider);

            //This throws if you are not connected to VPN
            Assert.DoesNotThrow(idCard.ValidateSignatureAndTrust);
        }

        [Test]
        public void ValidateVocesSignatureTest()
        {
            var factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate);
            var idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);
            idCard.Sign<Assertion>(factory.SignatureProvider);

            //This throws if you are not connected to VPN
            Assert.DoesNotThrow(idCard.ValidateSignatureAndTrust);
        }

        [Test]
        public void ValidateFocesSignatureTest()
        {
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.FocesValid.Certificate);
            var idCard = CreateVocesSystemIdCard(CertificateSuite.FocesValid); 
            idCard.Sign<Assertion>(factory.SignatureProvider);

            //This throws if you are not connected to VPN
            Assert.DoesNotThrow(idCard.ValidateSignatureAndTrust);
        }

        [Test]
        public void ValidateSignatureNegativeTest()
        {
            //Get invalid certificate
            X509Certificate2 newCert = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources", "oces2", "PP", "MOCES_udloebet.p12"), "Test1234"); // TODO Det er ikke muligt at teste OCES3 varianten før vi har et udløbet certifikat. Det har vi ikke pr. 25. januar 2022.
            SOSIFactory factory = CreateSOSIFactory(newCert);
            UserIdCard idCard = CreateMocesUserIdCard();
            idCard.Sign<Assertion>(factory.SignatureProvider);

            Assert.Throws<ModelException>(delegate { idCard.InternalValidateSignature(null, true); });
		}

		[Test]
        public void IdCardVocesSignTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate);

            //Create IdCard
            SystemIdCard idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);

            //Sign IdCard
            Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);
            Assertion ass2 = idCard.GetAssertion<Assertion>();

            //Assert assertion was created succesfully
            Assert.NotNull(ass);
            Assert.NotNull(idCard.Xassertion);

            //Make sure the assertion returned from Sign and Get are the same.
            Assert.True(ass.Signature.SignatureValue.ToString() == ass2.Signature.SignatureValue.ToString());
        }
    }
}
