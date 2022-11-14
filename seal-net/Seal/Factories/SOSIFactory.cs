using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Model.ModelBuilders;
using dk.nsi.seal.pki;

namespace dk.nsi.seal.Factories
{
    public class SOSIFactory
    {
        public ISignatureProvider SignatureProvider { get; }
        public Federation.Federation Federation { get; }

        public SOSIFactory(Federation.Federation federation, ISignatureProvider signatureProvider)
        {
            Federation = federation;
            SignatureProvider = signatureProvider;
        }

        public X509Certificate2 GetCertificate()
        {
	        var credentialVaultSignatureProvider = SignatureProvider as CredentialVaultSignatureProvider;
	        return credentialVaultSignatureProvider?.Vault.GetSystemCredentials();
        }

        public static SystemIdCard CreateNewSystemIdCard(string itSystemName, CareProvider careProvider, AuthenticationLevel authenticationLevel, string username, string password,
            X509Certificate2 certificate, string alternativeIdentifier)
        {
            var systemInfo = new SystemInfo(careProvider, itSystemName);
            return new SystemIdCard(Configuration.SosiDgwsVersion, authenticationLevel, Configuration.SosiIssuer, systemInfo, certificate?.GetCertHashString(), alternativeIdentifier, username, password);
        }

        public static UserIdCard CreateNewUserIdCard(string itSystemName, UserInfo userInfo, CareProvider careProvider, AuthenticationLevel authenticationLevel, string username,
            string password, X509Certificate2 certificate, string alternativeIdentifier)
        {
            var systemInfo = new SystemInfo(careProvider, itSystemName);
            return new UserIdCard(Configuration.SosiDgwsVersion, authenticationLevel, Configuration.SosiIssuer, systemInfo, userInfo, certificate?.GetCertHashString(), alternativeIdentifier, username, password);
        }

        public static IdCard DeserializeIdCard<T>(T assertion)
        {
            var builder = new IdCardModelBuilder();
            return builder.BuildModel(SerializerUtil.Serialize(assertion).Root);
        }
    }
}
