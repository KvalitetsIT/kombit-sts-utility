using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public class OCESUtil
    {
        public static bool IsProbableOCES3Certificate(X509Certificate2 certificate)
        {
            return certificate.Issuer.Contains("Den Danske Stat OCES");
        }
    }
}
