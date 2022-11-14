using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Model.ModelBuilders;
using NUnit.Framework;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateSuiteOces2))]
    [TestFixture(typeof(CertificateSuiteOces3))]
    public class SOSIFactoryTest<CERTIFICATE_SUITE> : AbstractTest where CERTIFICATE_SUITE : ICertificateSuite, new()
    {
        private CERTIFICATE_SUITE CertificateSuite;

        public SOSIFactoryTest()
        {
            CertificateSuite = new CERTIFICATE_SUITE();
        }
        [Test]
        public void CreateUserIdCardTest()
        {
            //Create IdCard
            SystemIdCard idCard = SOSIFactory.CreateNewUserIdCard("Sygdom.dk", new UserInfo("2408631478", "Amaja", "Christiansen", "jso@trifork.com", "Læge", "5175", "5GXFR"), new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.MocesTrustedUser, null, null, Global.MocesCprGyldig, null);
            Assert.NotNull(idCard);
        }

        [Test]
        public void CreateSystemIdCardTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate); 

            //Create IdCard
            SystemIdCard idCard = SOSIFactory.CreateNewSystemIdCard("ItSystem", new CareProvider(SubjectIdentifierType.medcomitsystemname, "TestSystem", "Trifork"), AuthenticationLevel.VocesTrustedSystem, "user", "test123", null, "alt");
            Assert.NotNull(idCard);
        }


        [Test]
        public void DeserializeUnsignedUserIdCardTest()
        {
            //Create IdCard
            UserIdCard idCard = CreateMocesUserIdCard();

            Assertion assertion = idCard.GetAssertion<Assertion>();

            UserIdCard deserializedCard = (UserIdCard)SOSIFactory.DeserializeIdCard(assertion);

            //Assert they are equal
            Assert.True(idCard.CreatedDate == deserializedCard.CreatedDate);
            Assert.True(idCard.ExpiryDate == deserializedCard.ExpiryDate);
            Assert.True(idCard.IsValidInTime == deserializedCard.IsValidInTime);
            Assert.True(idCard.UserInfo.Equals(deserializedCard.UserInfo));
            Assert.True(idCard.AuthenticationLevel.Equals(deserializedCard.AuthenticationLevel));
            Assert.True(idCard.CertHash == deserializedCard.CertHash);
            Assert.True(idCard.AlternativeIdentifier == deserializedCard.AlternativeIdentifier);
            Assert.True(idCard.IdCardId == deserializedCard.IdCardId);
            Assert.True(idCard.Issuer == deserializedCard.Issuer);
            Assert.True(idCard.Username == deserializedCard.Username);
            Assert.True(idCard.Password == deserializedCard.Password);
            Assert.True(idCard.SystemInfo.ItSystemName == deserializedCard.SystemInfo.ItSystemName);
            Assert.True(idCard.SystemInfo.CareProvider.Equals(deserializedCard.SystemInfo.CareProvider));
            Assert.True(idCard.Version == deserializedCard.Version);
            Assert.Throws<ModelBuildException>(delegate { var cert = deserializedCard.SignedByCertificate; });
        }

        [Test]
        public void DeserializeUnsignedSystemIdCardTest()
        {
            //Create IdCard
            SystemIdCard idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);

            Assertion assertion = idCard.GetAssertion<Assertion>();

            SystemIdCard deserializedCard = (SystemIdCard)SOSIFactory.DeserializeIdCard(assertion);

            //Assert they are equal
            Assert.True(idCard.CreatedDate == deserializedCard.CreatedDate);
            Assert.True(idCard.ExpiryDate == deserializedCard.ExpiryDate);
            Assert.True(idCard.IsValidInTime == deserializedCard.IsValidInTime);
            Assert.True(idCard.AuthenticationLevel.Equals(deserializedCard.AuthenticationLevel));
            Assert.True(idCard.CertHash == deserializedCard.CertHash);
            Assert.True(idCard.AlternativeIdentifier == deserializedCard.AlternativeIdentifier);
            Assert.True(idCard.IdCardId == deserializedCard.IdCardId);
            Assert.True(idCard.Issuer == deserializedCard.Issuer);
            Assert.True(idCard.Username == deserializedCard.Username);
            Assert.True(idCard.Password == deserializedCard.Password);
            Assert.True(idCard.SystemInfo.ItSystemName == deserializedCard.SystemInfo.ItSystemName);
            Assert.True(idCard.SystemInfo.CareProvider.Equals(deserializedCard.SystemInfo.CareProvider));
            Assert.True(idCard.Version == deserializedCard.Version);
            Assert.Throws<ModelBuildException>(delegate { var cert = deserializedCard.SignedByCertificate; });
        }

        [Test]
        public void DeserializeSignedSystemIdCardTest()
        {
            //Create Factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate);

            //Create IdCard
            SystemIdCard idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);
            idCard.Sign<Assertion>(factory.SignatureProvider);

            Assertion assertion = idCard.GetAssertion<Assertion>();

            SystemIdCard deserializedCard = (SystemIdCard)SOSIFactory.DeserializeIdCard(assertion);

            //Assert they are equal
            Assert.True(idCard.CreatedDate == deserializedCard.CreatedDate);
            Assert.True(idCard.ExpiryDate == deserializedCard.ExpiryDate);
            Assert.True(idCard.IsValidInTime == deserializedCard.IsValidInTime);
            Assert.True(idCard.AuthenticationLevel.Equals(deserializedCard.AuthenticationLevel));
            Assert.True(idCard.CertHash == deserializedCard.CertHash);
            Assert.True(idCard.AlternativeIdentifier == deserializedCard.AlternativeIdentifier);
            Assert.True(idCard.IdCardId == deserializedCard.IdCardId);
            Assert.True(idCard.Issuer == deserializedCard.Issuer);
            Assert.True(idCard.Username == deserializedCard.Username);
            Assert.True(idCard.Password == deserializedCard.Password);
            Assert.True(idCard.SystemInfo.ItSystemName == deserializedCard.SystemInfo.ItSystemName);
            Assert.True(idCard.SystemInfo.CareProvider.Equals(deserializedCard.SystemInfo.CareProvider));
            Assert.True(idCard.Version == deserializedCard.Version);
            Assert.True(idCard.SignedByCertificate.Equals(deserializedCard.SignedByCertificate));
        }

        [Test]
        public void DeserializeSignedUserIdCardTest()
        {
            //Create Factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard
            UserIdCard idCard = CreateMocesUserIdCard();
            idCard.Sign<Assertion>(factory.SignatureProvider);

            Assertion assertion = idCard.GetAssertion<Assertion>();

            UserIdCard deserializedCard = (UserIdCard)SOSIFactory.DeserializeIdCard(assertion);

            //Assert they are equal
            Assert.True(idCard.CreatedDate == deserializedCard.CreatedDate);
            Assert.True(idCard.ExpiryDate == deserializedCard.ExpiryDate);
            Assert.True(idCard.IsValidInTime == deserializedCard.IsValidInTime);
            Assert.True(idCard.UserInfo.Equals(deserializedCard.UserInfo));
            Assert.True(idCard.AuthenticationLevel.Equals(deserializedCard.AuthenticationLevel));
            Assert.True(idCard.CertHash == deserializedCard.CertHash);
            Assert.True(idCard.AlternativeIdentifier == deserializedCard.AlternativeIdentifier);
            Assert.True(idCard.IdCardId == deserializedCard.IdCardId);
            Assert.True(idCard.Issuer == deserializedCard.Issuer);
            Assert.True(idCard.Username == deserializedCard.Username);
            Assert.True(idCard.Password == deserializedCard.Password);
            Assert.True(idCard.SystemInfo.ItSystemName == deserializedCard.SystemInfo.ItSystemName);
            Assert.True(idCard.SystemInfo.CareProvider.Equals(deserializedCard.SystemInfo.CareProvider));
            Assert.True(idCard.Version == deserializedCard.Version);
            Assert.True(idCard.SignedByCertificate.Equals(deserializedCard.SignedByCertificate));
        }
    }
}
