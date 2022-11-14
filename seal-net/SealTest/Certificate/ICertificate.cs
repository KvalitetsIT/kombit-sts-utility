using dk.nsi.seal.Vault;
using System.Security.Cryptography.X509Certificates;

namespace SealTest.Certificate
{
    public interface ICertificate
    {
        X509Certificate2 Certificate { get; }
        ICredentialVault CredentialVault { get; }
        string CertificatePath { get; }
        string CertificatePassword { get; }
        string Cvr { get; }
        string Cpr { get; }
    }
}
