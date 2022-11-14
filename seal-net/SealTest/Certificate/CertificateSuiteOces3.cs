namespace SealTest.Certificate
{
    public class CertificateSuiteOces3 : ICertificateSuite
    {
        public ICertificate VocesValid => new CertificateOces3Voces();
        public ICertificate FocesValid => new CertificateOces3Foces();
        public ICertificate FocesRevoked => new CertificateOces3FocesRevoked();
        public ICertificate FocesExpired => new CertificateOces3FocesExpired();
        public ICertificate MocesCprValid => new CertificateOces3MocesCpr();
        public ICertificate MocesRevoked => new CertificateOces3MocesRevokedCpr();
    }
}
