using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Federation;
using dk.nsi.seal.pki;
using dk.nsi.seal.Vault;
using SealTest.Certificate;

namespace SealTest.Model
{
    public abstract class AbstractTest
    {
        public static SOSIFactory CreateSOSIFactory(X509Certificate2 cert)
        {
			InMemoryCredentialVault vault = new(cert);

			CredentialVaultSignatureProvider sigProvider = new(vault);
            SOSIFactory factory = new(null, sigProvider);
            return factory;
        }

        public SOSIFactory CreateSOSIFactoryWithTestFederation(X509Certificate2 cert)
        {
            SosiTestFederation federation = new(new CrlCertificateStatusChecker());
			InMemoryCredentialVault vault = new(cert);

			CredentialVaultSignatureProvider sigProvider = new(vault);
            SOSIFactory factory = new(federation, sigProvider);
            return factory;
        }

        public SOSIFactory CreateSOSIFactoryWithSosiFederation(X509Certificate2 cert)
        {
            SosiFederation federation = new(new CrlCertificateStatusChecker());
            InMemoryCredentialVault vault = new(cert);

            CredentialVaultSignatureProvider sigProvider = new(vault);
            SOSIFactory factory = new(federation, sigProvider);
            return factory;
        }


        public UserIdCard CreateMocesUserIdCard() => 
            SOSIFactory.CreateNewUserIdCard("Sygdom.dk", new UserInfo("2408631478", "Amaja", "Christiansen", "jso@trifork.com", "Læge", "5175", "5GXFR"), new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.MocesTrustedUser, null, null, Global.MocesCprGyldig, null);

        public static UserIdCard CreateUserIdCard(SOSIFactory factory, string userName, string passWord) => 
            SOSIFactory.CreateNewUserIdCard("ItSystem", new UserInfo("12345678", "Test", "Person", "test@person.dk", "Tester", "Læge", "12345"), new CareProvider(SubjectIdentifierType.medcomcvrnumber, "25520041", "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.UsernamePasswordAuthentication, userName, passWord, factory.GetCertificate(), "alt");

        public static SystemIdCard CreateVocesSystemIdCard(ICertificate certificate) => 
            SOSIFactory.CreateNewSystemIdCard("ItSystem", new CareProvider(SubjectIdentifierType.medcomcvrnumber, certificate.Cvr, "TRIFORK SERVICES A/S // CVR:25520041"), AuthenticationLevel.VocesTrustedSystem, null, null, certificate.Certificate, "alt");

        public static UserIdCard CreateIdCardForSTS(ICertificate certificate, string cpr = "1802602810", string authorization = "ZXCVB") => 
            SOSIFactory.CreateNewUserIdCard("Sygdom.dk", new UserInfo(cpr, "Stine", "Svendsen", "stineSvendsen@example.com", "læge", "7170", authorization), new CareProvider(SubjectIdentifierType.medcomcvrnumber, certificate.Cvr, "Statens Serum Institut"), AuthenticationLevel.MocesTrustedUser, "", "", certificate.Certificate, "");
    }
}
