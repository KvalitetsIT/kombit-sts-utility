using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace dk.nsi.seal.Federation
{
    public class Oces2DistinguishedName : DistinguishedName
    {
        public string DistinguishedNameString { get; }

        /**
         * Construct an instance of <code>DistinguishedName</code>
         *
         * @param distinguishedName string representation of DN
         * @throws PKIException if DN parsing fails
         */
        public Oces2DistinguishedName(string distinguishedName)
        {
            DistinguishedNameString = distinguishedName;
            Parse();
        }

        /**
         * Construct an instance of <code>DistinguishedName</code>
         *
         * @param principal the X500Principal to extract the DN for
         * @throws PKIException if DN parsing fails
         */
        public Oces2DistinguishedName(X500DistinguishedName principal)
        {
            DistinguishedNameString = principal.Name;
            Parse();
        }

        public string CommonName { get; private set; }

        public string Country { get; private set; }

        public string Organization { get; private set; }

        public string SubjectSerialNumber { get; private set; }


        private void Parse()
        {
            try
            {
                var cPattern = @"C=(?<c>.+)(?<!\\)";
                var cMatch = Regex.Match(DistinguishedNameString, cPattern);
                Country = cMatch.Groups[1].Value;

                var oPattern = @"O=(?<o>.+)(?<!\\),";
                var oMatch = Regex.Match(DistinguishedNameString, oPattern);
                Organization = oMatch.Groups[1].Value;

                var cnPattern = @"CN=(?<cn>.+)(?<!\\),";
                var cnMatch = Regex.Match(DistinguishedNameString, cnPattern);
                CommonName = cnMatch.Groups[1].Value;

                var serialPattern = @"^SERIALNUMBER=(?<serial>.+)(?<!\\)\ \+";
                var serialMatch = Regex.Match(DistinguishedNameString, serialPattern);
                SubjectSerialNumber = serialMatch.Groups[1].Value;
            }
            catch (Exception e)
            {
                throw;
            }

        }
    }
}
