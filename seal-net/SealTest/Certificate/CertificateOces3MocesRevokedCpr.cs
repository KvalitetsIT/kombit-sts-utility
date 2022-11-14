using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;

namespace SealTest.Certificate
{
    internal class CertificateOces3MocesRevokedCpr : ICertificate
    {
        public X509Certificate2 Certificate => new(CertificatePath, CertificatePassword);
        public string CertificatePath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","oces3","devtest4","MOCES_spaerret.p12");
        public string CertificatePassword => "Test1234";
        public ICredentialVault CredentialVault => CredentialVaultTestUtil.GetCredentialVault(CertificatePath, CertificatePassword);

        public string Cvr => throw new NotImplementedException();

        public string Cpr => throw new NotImplementedException();
    }
}