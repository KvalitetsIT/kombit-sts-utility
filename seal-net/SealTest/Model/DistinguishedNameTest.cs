using dk.nsi.seal.Federation;
using NUnit.Framework;
using SealTest.Certificate;

namespace SealTest.Model
{
    [TestFixture(typeof(CertificateOces2Voces), "DK", "NETS DANID A/S // CVR:30808460", "CVR:30808460-UID:25351738", "NETS DANID A/S - TU VOCES gyldig, O=NETS DANID A/S // CVR:30808460")]
    [TestFixture(typeof(CertificateOces3Voces), "DK", "Testorganisation nr. 94354969", "UI:DK-O:G:c70b0207-162e-4d3d-a7f1-a19a8e07d99b", "VOCES_gyldig")]
    [TestFixture(typeof(CertificateOces2MocesCpr), "DK", "NETS DANID A/S // CVR:30808460", "CVR:30808460-RID:42634739", "TU GENEREL MOCES M CPR gyldig, O=NETS DANID A/S // CVR:30808460")]
    [TestFixture(typeof(CertificateOces3MocesCpr), "DK", "Testorganisation nr. 91026150", "UI:DK-E:G:dc7f4a98-d4f9-4d77-84f9-ae83c4e0bf61", "Thormund Nissen")]
    public class DistinguishedNameTest<CERTIFICATE> where CERTIFICATE : ICertificate, new()
    {
        private readonly string expectedCountry;
        private readonly string expectedOrganisation;
        private readonly string expectedSerial;
        private readonly string expectedCommonName;
        private CERTIFICATE certificate;
        private DistinguishedName distinguishedNameFromX500DistinguishedName;
        private DistinguishedName distinguishedNameFromString;

        public DistinguishedNameTest(string expectedCountry, string expectedOrganisation, string expectedSerial, string expectedCommonName)
        {
            this.expectedCountry = expectedCountry;
            this.expectedOrganisation = expectedOrganisation;
            this.expectedSerial = expectedSerial;
            this.expectedCommonName = expectedCommonName;

            certificate = new CERTIFICATE();
        }

        [SetUp]
        public void setup()
        {
            if(OCESUtil.IsProbableOCES3Certificate(certificate.Certificate))
            {
                distinguishedNameFromX500DistinguishedName = new Oces3DistinguishedName(certificate.Certificate.SubjectName);
                distinguishedNameFromString = new Oces3DistinguishedName(certificate.Certificate.SubjectName.Name);
            }
            else
            {
                distinguishedNameFromX500DistinguishedName = new Oces2DistinguishedName(certificate.Certificate.SubjectName);
                distinguishedNameFromString = new Oces2DistinguishedName(certificate.Certificate.SubjectName.Name);
            }
        }

        [Test]
        public void testParseDistinguishedNameFromCertificate()
        {
            Assert.AreEqual(expectedCountry, distinguishedNameFromX500DistinguishedName.Country);
            Assert.AreEqual(expectedOrganisation, distinguishedNameFromX500DistinguishedName.Organization);
            Assert.AreEqual(expectedSerial, distinguishedNameFromX500DistinguishedName.SubjectSerialNumber);
            Assert.AreEqual(expectedCommonName, distinguishedNameFromX500DistinguishedName.CommonName);
        }

        [Test]
        public void testParseDistinguishedNameFromString()
        {
            Assert.AreEqual(expectedCountry, distinguishedNameFromString.Country);
            Assert.AreEqual(expectedOrganisation, distinguishedNameFromString.Organization);
            Assert.AreEqual(expectedSerial, distinguishedNameFromString.SubjectSerialNumber);
            Assert.AreEqual(expectedCommonName, distinguishedNameFromString.CommonName);
        }

    }
}
