using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public interface ICertificationAuthority
    {
        /**
         * Perform revocation check of the passed certificate using the configured CertificateStatusChecker
         *
         * @param certificate
         * @return true if the passed certificate is issued by this CA and is not revoked and is within its validity period. false otherwise
         */
        bool IsValid(X509Certificate2 certificate);

        /**
         * Perform revocation check of the passed certificate using the configured CertificateStatusChecker.
         *
         *
         * @param certificate to be checked
         * @return a <code>CertificateStatus</code> for the supplied certificate.
         */
        CertificateStatus GetCertificateStatus(X509Certificate2 certificate);
    }
}
