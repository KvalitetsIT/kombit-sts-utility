using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Vault
{
    public class InMemoryCredentialVault : ICredentialVault
    {
		private readonly X509Certificate2 cert;

        public InMemoryCredentialVault(X509Certificate2 certificate) => cert = certificate;

        public X509Certificate2 GetSystemCredentials() => cert;
    }
}
