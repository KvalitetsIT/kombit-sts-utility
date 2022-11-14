using dk.nsi.seal.Federation;
using NUnit.Framework;

namespace SealTest.FederationTest
{
    class OcesCertificationAuthorityTest
    {
        [Test]
        public void TestOces3RootValidFormat()
        {
            var rootCertificate = OcesCertificationAuthority.Oces3RootCertificate;
            Assert.AreEqual("C=DK, O=Den Danske Stat, CN=Den Danske Stat OCES rod-CA", rootCertificate.Subject);
        }
    }
}
