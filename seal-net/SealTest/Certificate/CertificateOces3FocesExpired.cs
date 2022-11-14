using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;

namespace SealTest.Certificate
{
    public class CertificateOces3FocesExpired : ICertificate
    {
        public X509Certificate2 Certificate => null;

        public string CertificatePath => null;

        public string CertificatePassword => null;

        public ICredentialVault CredentialVault => throw new System.NotImplementedException();

        public string Cvr => throw new System.NotImplementedException();

        public string Cpr => throw new System.NotImplementedException();
    }
}
