using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Vault
{
	public class ThumbprintCertStoreCredentialVault : ICredentialVault
	{
		protected string thumbprint;
		protected X509Store certStore;

		public ThumbprintCertStoreCredentialVault(string thumbprint, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
		{
			this.thumbprint = thumbprint;
			this.certStore = new X509Store(storeName, storeLocation);
		}

		public X509Certificate2 GetSystemCredentials()
		{
			certStore.Open(OpenFlags.ReadOnly);
			var resultCertCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
			certStore.Close();
			if(resultCertCollection.Count == 1)
			{
				return resultCertCollection[0];
			}
			return null;
		}
	}
}
