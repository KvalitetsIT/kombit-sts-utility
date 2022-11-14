using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.pki
{
    public class SignatureResult
    {
        public string Signature { get; }
        public X509Certificate2 Certificate { get; }

        public SignatureResult(string signature, X509Certificate2 cert)
        {
            Signature = signature;
            Certificate = cert;
        }
    }
}
