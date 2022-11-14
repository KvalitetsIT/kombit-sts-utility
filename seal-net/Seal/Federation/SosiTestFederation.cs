namespace dk.nsi.seal.Federation
{
    public class SosiTestFederation : Federation
    {
        private const string NewFocesTestStsSubjectNamePrefix = "SOSI Test Federation";
        private SosiStsCertificateMatcher matcher;
        public SosiTestFederation(ICertificateStatusChecker certificateChecker) : base(new OcesTestCertificationAuthority(certificateChecker))
        {
            matcher = new SosiStsCertificateMatcher(NewFocesTestStsSubjectNamePrefix);
        }


        protected override bool SubjectDistinguishedNameMatches(DistinguishedName subjectDistinguishedName)
        {
            return matcher.Matches(subjectDistinguishedName);
        }
    }
}
