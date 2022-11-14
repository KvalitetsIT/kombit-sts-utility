namespace SealTest.Certificate
{
    public interface ICertificateSuite
    {
        ICertificate VocesValid { get; }
        ICertificate FocesValid { get; }
        ICertificate FocesRevoked { get; }
        ICertificate FocesExpired { get; }
        ICertificate MocesCprValid { get; }
        ICertificate MocesRevoked { get; }
    }
}
