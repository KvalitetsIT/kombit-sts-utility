using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using NUnit.Framework;

namespace SealTest
{
    public class Global
    {
        public static X509Certificate2 cert => new(TestContext.CurrentContext.TestDirectory + "/Resources/VicValidVOCES.p12", "!234Qwer");

        public static X509Certificate2 VocesGyldig => new(TestContext.CurrentContext.TestDirectory + "/Resources/oces2/PP/VOCES_gyldig.p12", "Test1234");

        public static X509Certificate2 MocesCprGyldig => new(TestContext.CurrentContext.TestDirectory + "/Resources/oces2/PP/MOCES_cpr_gyldig.p12", "Test1234");

        public static X509Certificate2 StatensSerumInstitutFoces => new(TestContext.CurrentContext.TestDirectory + "/Resources/certificates/Statens_Serum_Institut_FOCES.p12", "Test1234");

        public static string[] AuthIds = { "NS101", "NS102", "NS103" };
        public static string[] PatientCprs = { "0411427781", "2911245178", "0510171632", "1403713968", "2908993384", "1703056748" };


        public static XElement SignedTokenXml() => XElement.Load(TestContext.CurrentContext.TestDirectory + "/Resources/SignedToken.xml");
    }
}