using dk.nsi.fmk;
using dk.nsi.seal;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Serializers;
using NUnit.Framework;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateSuiteOces2))]
    [TestFixture(typeof(CertificateSuiteOces3))]
    public class SerializationTest<CERTIFICATE_SUITE> : AbstractTest where CERTIFICATE_SUITE : ICertificateSuite, new()
	{
        private readonly CERTIFICATE_SUITE CertificateSuite;

        public SerializationTest()
        {
            CertificateSuite = new CERTIFICATE_SUITE();
        }


		[Test]
		public void UserIdCardSerializeStringTest()
		{
			//Create factory
			SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

			//Create IdCard
			UserIdCard idCard = CreateMocesUserIdCard();

			//Sign IdCard
			Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);

			var idCardString = IdCardSerializer.SerializeIdCardToString<UserIdCard>(idCard);
			var newIdCard = IdCardSerializer.DeserializeIdCard<UserIdCard>(idCardString);

			Assertion.Equals(idCard, newIdCard);
		}

		[Test]
		public void UserIdCardSerializeStreamTest()
		{
			//Create factory
			SOSIFactory factory = CreateSOSIFactory(CertificateSuite.MocesCprValid.Certificate);

            //Create IdCard
            UserIdCard idCard = CreateMocesUserIdCard();

			//Sign IdCard
			Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);

			var idCardStream = IdCardSerializer.SerializeIdCardToStream<UserIdCard>(idCard);
			var newIdCard = IdCardSerializer.DeserializeIdCard<UserIdCard>(idCardStream);

			Assertion.Equals(idCard, newIdCard);
		}


        [Test]
        public void SystemIdCardSerializeStringTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate);

            //Create IdCard
            var idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);

            //Sign IdCard
            Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);

            var idCardString = IdCardSerializer.SerializeIdCardToString<SystemIdCard>(idCard);
            var newIdCard = IdCardSerializer.DeserializeIdCard<SystemIdCard>(idCardString);

            Assertion.Equals(idCard, newIdCard);
        }

        [Test]
        public void SystemIdCardSerializeStreamTest()
        {
            //Create factory
            SOSIFactory factory = CreateSOSIFactory(CertificateSuite.VocesValid.Certificate);

            //Create IdCard
            var idCard = CreateVocesSystemIdCard(CertificateSuite.VocesValid);

            //Sign IdCard
            Assertion ass = idCard.Sign<Assertion>(factory.SignatureProvider);

            var idCardStream = IdCardSerializer.SerializeIdCardToStream<SystemIdCard>(idCard);
            var newIdCard = IdCardSerializer.DeserializeIdCard<SystemIdCard>(idCardStream);

            Assertion.Equals(idCard, newIdCard);
        }
    }
}
