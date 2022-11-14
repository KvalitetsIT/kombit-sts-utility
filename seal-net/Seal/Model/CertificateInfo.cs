using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace dk.nsi.seal.Model
{
    public class CertificateInfo
    {
        private const string Pattern = "SubjectDN=\\{([^\\{}]+)\\},IssuerDN=\\{([^\\{}]+)},CertSerial=\\{([^\\{}]+)}";

        public X500DistinguishedName SubjectDn { get; }
        public X500DistinguishedName IssuerDn { get; }
        public string CertificateSerial { get; }
        public string SubjectSerialNumber { get; private set; }
        public string RidNumber { get; private set; }

        public CertificateInfo(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentException("certificate must not be null!");
            }
            SubjectDn = new X500DistinguishedName(certificate.Subject);
            IssuerDn = new X500DistinguishedName(certificate.Issuer);

            CertificateSerial = certificate.GetSerialNumberString();

            ExtractMoreInfo();
        }

        private void ExtractMoreInfo()
        {
            SubjectSerialNumber = GetSeialFromSubjectDn(SubjectDn);
            RidNumber = ExtractRidNumber(SubjectSerialNumber);
        }


        public static string GetSeialFromSubjectDn(X500DistinguishedName subjectDn)
        {
            var serialPattern = @"SERIALNUMBER=(?<serial>.+?)[ +,]";
            const string pattern = "SERIALNUMBER";
            var reversedPattern = new string(pattern.Reverse().ToArray());

            var removedTextBeforeSerialNumber = new string(subjectDn.Name.Reverse()
                                                                         .TakeUntilFirstOccurenceOfPattern(reversedPattern)
                                                                         .Reverse()
                                                                         .ToArray());
            var serialMatch = Regex.Match(removedTextBeforeSerialNumber, serialPattern, RegexOptions.ExplicitCapture);
            return new string(serialMatch.Groups[1].Value.Reverse().Reverse().ToArray());
        }

        public static CertificateInfo FromString(string certInfoString)
        {
            if (certInfoString == null)
            {
                throw new ArgumentException("certInfoString must not be null!");
            }
            Match match = Regex.Match(certInfoString, Pattern);
            if (!match.Success)
            {
                throw new ArgumentException("certInfoString does not represent a CertificateInfo, certInfoString was '"
                + certInfoString + "'");
            }
            return new CertificateInfo(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }

        public static bool IsProbableCertificateInfoString(string candidate)
        {
            return candidate != null && Regex.Match(candidate, Pattern).Success;
        }

        private CertificateInfo(string subjectDNString, string issuerDNString, string certificateSerialString)
        {
            this.SubjectDn = new X500DistinguishedName(subjectDNString);
            this.IssuerDn = new X500DistinguishedName(issuerDNString);
            this.CertificateSerial = certificateSerialString;

            ExtractMoreInfo();
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("SubjectDN={");
            builder.Append(SubjectDn.Name);
            builder.Append("},IssuerDN={");
            builder.Append(IssuerDn.Name);
            builder.Append("},CertSerial={");
            builder.Append(CertificateSerial);
            builder.Append("}");
            return builder.ToString();
        }

        private string ExtractRidNumber(string subjectSerialNumber)
        {
            String ridIdentifier = "RID:";
            int index = subjectSerialNumber.IndexOf(ridIdentifier);
            if (index == -1)
            {
                throw new ModelException("Could not extract RID number from subject serial number: '" + subjectSerialNumber + "'");
            }
            else
            {
                return subjectSerialNumber.Substring(index + ridIdentifier.Length);
            }
        }
    }

    public static class StringExt
    {
        public static string TakeUntilFirstOccurenceOfPattern(this IEnumerable<char> @this, string pattern) =>
            @this.Aggregate("", (acc, s) => acc.EndsWith(pattern) ?
                                            acc :
                                            acc + s);
    }
}
