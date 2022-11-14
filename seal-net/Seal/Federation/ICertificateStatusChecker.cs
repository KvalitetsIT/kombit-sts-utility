using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public interface ICertificateStatusChecker
    {
        CertificateStatus GetRevocationStatus(X509Certificate2 certificate);
    }
}
