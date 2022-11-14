using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;

namespace SealTest.Certificate
{
    internal class CertificateOces3MocesCpr : ICertificate
    {
        public X509Certificate2 Certificate => new(CertificatePath, CertificatePassword);
        public string CertificatePath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","oces3","devtest4","MOCES_cpr_gyldig.p12");
        public string CertificatePassword => "Test1234";
        public ICredentialVault CredentialVault => CredentialVaultTestUtil.GetCredentialVault(CertificatePath, CertificatePassword);

        public string Cvr => "91026150";

        public string Cpr => throw new NotImplementedException();
    }
}