using System.Xml.Linq;
using dk.nsi.seal.Constants;

namespace dk.nsi.seal.Model.DomBuilders
{
    public class OioSamlAssertionToIdCardRequestDomBuilder : OioWsTrustRequestDomBuilder
	{

		private const string SOSI_FEDERATION = "http://sosi.dk";

		public OioSamlAssertion OioSamlAssertion { get; set; }
		public string UserAuthorizationCode { get; set; }
		public string UserEducationCode { get; set; }
		public string UserGivenName { get; set; }
		public string UserSurName { get; set; }
		public string ItSystemName { get; set; }
		
		public OioSamlAssertionToIdCardRequestDomBuilder() : base()
		{
			this.Audience = SOSI_FEDERATION;
		}

		protected override void ValidateBeforeBuild()
		{
			base.ValidateBeforeBuild();

			Validate("OIOSAMLAssertion", OioSamlAssertion);

			Validate("itSystemName", ItSystemName);

			Validate("signingVault", SigningVault);

			ValidateValue("userAuthorizationCode", UserAuthorizationCode);

			ValidateValue("userRole", UserEducationCode);

			ValidateValue("userGivenName", UserGivenName);

			ValidateValue("userSurName", UserSurName);
		}


		protected override void AddActAsTokens(XElement actAs)
		{
			AddOioSamlAssertion(actAs);
			AddHealthcareContextToken(actAs);
		}

		private void AddOioSamlAssertion(XElement actAs)
		{
			AddAssertion(actAs, OioSamlAssertion);
		}


		private void AddHealthcareContextToken(XElement actAs)
		{
			var builder = new HealthcareContextTokenDomBuilder(UserGivenName)
			{
				Issuer = ItSystemName,
				ItSystem = ItSystemName,
				SubjectName = SigningVault.GetSystemCredentials().Subject,
				UserGivenName = UserGivenName,
				UserSurName = UserSurName,
				UserAuthorizationCode = UserAuthorizationCode,
				SubjectNameId = ItSystemName,
				SubjectNameIdFormat = SamlValues.NameidFormatX509SubjectName,
				UserEducationCode = UserEducationCode,
			};
			var healthCareContextToken = builder.Build();
			AddAssertion(actAs, healthCareContextToken);
		}

		private void AddAssertion(XElement actAs, OioSamlAssertion assertion)
		{
			actAs.Add(assertion.XAssertion);
		}

	}
}
