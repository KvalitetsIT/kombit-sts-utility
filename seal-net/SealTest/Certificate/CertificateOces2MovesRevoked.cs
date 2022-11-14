using dk.nsi.seal.Vault;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SealTest.Certificate
{
    internal class CertificateOces2MovesRevoked : ICertificate
    {
        public X509Certificate2 Certificate => new(CertificatePath, CertificatePassword);
        public string CertificatePath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","oces2","PP","MOCES_spaerret.p12");
        public string CertificatePassword => "Test1234";
        public ICredentialVault CredentialVault => CredentialVaultTestUtil.GetCredentialVault(CertificatePath, CertificatePassword);
        public string Cvr => "30808460";

        public string Cpr => throw new NotImplementedException();
    }
}