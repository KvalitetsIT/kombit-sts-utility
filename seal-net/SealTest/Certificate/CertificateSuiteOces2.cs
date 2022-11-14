namespace SealTest.Certificate
{
    public class CertificateSuiteOces2 : ICertificateSuite
    {
        public static CertificateSuiteOces2 Instance = new();
        public ICertificate VocesValid => new CertificateOces2Voces();
        public ICertificate FocesValid => new CertificateOces2Foces();
        public ICertificate FocesRevoked => new CertificateOces2FocesRevoked();
        public ICertificate FocesExpired => new CertificateOces2FocesExpired();
        public ICertificate MocesCprValid => new CertificateOces2MocesCpr();

        public ICertificate MocesRevoked => new CertificateOces2MovesRevoked();
    }
}
