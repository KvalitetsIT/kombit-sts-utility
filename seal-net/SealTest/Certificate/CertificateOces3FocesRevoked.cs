using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;

namespace SealTest.Certificate
{
    public class CertificateOces3FocesRevoked : ICertificate
    {
        public X509Certificate2 Certificate => new(CertificatePath, CertificatePassword);
        public string CertificatePath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","oces3","devtest4","FOCES_spaerret.p12");
        public string CertificatePassword => "Test1234";

        public ICredentialVault CredentialVault => throw new NotImplementedException();

        public string Cvr => "94354969";

        public string Cpr => throw new NotImplementedException();
    }
}
