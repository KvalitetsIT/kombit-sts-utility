using System;
using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{

	public class CrlCertificateStatusChecker : ICertificateStatusChecker
	{
		/**
		 * For this to work you have to import the system_vii.p12 file from SealTest/Resources/OCES2/PP as trusted root certificate.
		 * To do this open "Manage Computer Certificates" in windows.
		 * Then Action -> All Tasks -> Import
		 * Find the certificate and import it into "Trusted Root Certification Authorities"
		 * Then it should work
		 */
		public CertificateStatus GetRevocationStatus(X509Certificate2 certificate)
		{
			if (certificate == null) throw new ArgumentNullException(nameof(certificate));

			if (certificate.Verify())
				return new CertificateStatus(true);

			return new CertificateStatus(false);
		}
	}
}
