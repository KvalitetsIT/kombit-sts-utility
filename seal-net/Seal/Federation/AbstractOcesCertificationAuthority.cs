using System;
using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public abstract class AbstractOcesCertificationAuthority : ICertificationAuthority
    {
        private readonly ICertificateStatusChecker CertificateStatusChecker;

        public AbstractOcesCertificationAuthority(ICertificateStatusChecker certificateStatusChecker)
        {
            if (certificateStatusChecker == null)
                throw new ArgumentException("'certificateStatusChecker' cannot be null");
            CertificateStatusChecker = certificateStatusChecker;
        }

        protected abstract X509Certificate2 GetOCES2RootCertificate();
        protected abstract X509Certificate2 GetOCES3RootCertificate();

        protected abstract string GetCertificationAuthorityName();

        public bool IsValid(X509Certificate2 certificate) => GetCertificateStatus(certificate).IsValid;

        public CertificateStatus GetCertificateStatus(X509Certificate2 certificate)
        {
            if (!CheckDates(certificate)) { return new CertificateStatus(false); }
            return CompareWithRoot(certificate)
                ? CertificateStatusChecker.GetRevocationStatus(certificate)
                : new CertificateStatus(false);
        }

        private bool CheckDates(X509Certificate2 certificate)
        {
            if (certificate.NotAfter < DateTime.Now)
            {
                return false; // Certificate is expired
            }
            else if (certificate.NotBefore > DateTime.Now)
            {
                return false; // Certificate is not yet valid
            }

            return true;
        }

        private bool CompareWithRoot(X509Certificate2 certificateToValidate)
        {
            var authority = GetAuthority(certificateToValidate);

            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.ExtraStore.Add(authority);

            chain.Build(certificateToValidate);

            var chainRoot = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
            return chainRoot.Equals(authority);
        }

        private X509Certificate2 GetAuthority(X509Certificate2 certificateToValidate)
        {
            if (OCESUtil.IsProbableOCES3Certificate(certificateToValidate))
            {
                return GetOCES3RootCertificate();
            }

            return GetOCES2RootCertificate();
        }
    }
}