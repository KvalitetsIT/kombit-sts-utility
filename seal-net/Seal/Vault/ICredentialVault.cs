using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Vault
{
    public interface ICredentialVault
    {
        X509Certificate2 GetSystemCredentials();
    }

}
