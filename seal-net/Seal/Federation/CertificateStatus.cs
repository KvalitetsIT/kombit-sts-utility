namespace dk.nsi.seal.Federation
{
    public class CertificateStatus
    {
        public bool IsValid { get; }

        public CertificateStatus(bool isValid) => IsValid = isValid;
    }
}
