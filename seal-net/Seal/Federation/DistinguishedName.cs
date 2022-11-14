namespace dk.nsi.seal.Federation
{
    public interface DistinguishedName
    {
        string CommonName { get; }
        string SubjectSerialNumber { get; }
        string Country { get; }
        string Organization { get; }
    }
}
