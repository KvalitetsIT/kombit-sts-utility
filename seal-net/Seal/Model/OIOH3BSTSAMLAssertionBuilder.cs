using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.DomBuilders;
using System;
using System.Xml.Linq;
using static dk.nsi.seal.Model.OIOBSTSAMLAssertion;

namespace dk.nsi.seal.Model
{
    public class OIOH3BSTSAMLAssertionBuilder : AbstractOIOBSTSAMLAssertionBuilder<OIOH3BSTSAMLAssertion>
	{
		public OIOH3BSTSAMLAssertionBuilder()
		{
			AddAttribute(OIOH3BSTSAMLAssertion.SamlSpecVer, "OIO-SAML-3.0");
			AddAttribute(OIOH3BSTSAMLAssertion.SamlSpecVerAdditional, "OIO-SAML-H-3.0");
		}

		protected override string NameIdFormat => SamlValues.NameidFormatPersistent;

        public BasicPrivilegesDOMBuilder Privileges { get; set; }

        protected override SAMLAttribute[] GetAttributes()
		{
			return new SAMLAttribute[] {
				OIOH3BSTSAMLAssertion.SamlSpecVer,
				OIOH3BSTSAMLAssertion.SamlSpecVerAdditional,
				OIOH3BSTSAMLAssertion.SamlAssuranceLevel,
				OIOH3BSTSAMLAssertion.SamlProfessionalUuid,
				OIOH3BSTSAMLAssertion.SamlCvr,
				OIOH3BSTSAMLAssertion.SamlOrganizationName,
				OIOH3BSTSAMLAssertion.SamlEmail,
				OIOH3BSTSAMLAssertion.SamlAlias,
				OIOH3BSTSAMLAssertion.SamlPrivileges
			 };
		}

        public override void ValidateBeforeBuild()
        {
			if(Privileges!= null)
            {
				Privileges.PublicValidateBeforeBuild();
				AddPrivilegeAttribnute();
            }
            base.ValidateBeforeBuild();
        }

        private void AddPrivilegeAttribnute()
        {
			var priviliges = Privileges.Build();
			String privilegesB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(priviliges.ToString()));
			AddAttribute(OIOH3BSTSAMLAssertion.SamlPrivileges, privilegesB64);
		}

		public void SetAssuranceLevel(string type, string level)
		{
			switch (type)
			{
				case "NIST":
					RemoveAttribute(OIO3BSTSAMLAssertion.SamlNsisAssuranceLevel);
					AddAttribute(OIO3BSTSAMLAssertion.SamlNistAssuranceLevel, level);
					break;
				case "NSIS":
					RemoveAttribute(OIO3BSTSAMLAssertion.SamlNistAssuranceLevel);
					AddAttribute(OIO3BSTSAMLAssertion.SamlNsisAssuranceLevel, level);
					break;
				default:
					throw new ModelException("Unkown AssuranceLevel:" + type);
			}
		}

		public string Uuid { set => AddAttribute(OIOH3BSTSAMLAssertion.SamlProfessionalUuid, value); }
        public string Cvr { set => AddAttribute(OIOH3BSTSAMLAssertion.SamlCvr, value); }
		public string OrganizationName { set => AddAttribute(OIOH3BSTSAMLAssertion.SamlOrganizationName, value); }

		protected override OIOH3BSTSAMLAssertion CreateAssertion(XElement elm)
		{
			return new OIOH3BSTSAMLAssertion(elm);
		}
		protected override void CreateSubject(XElement assertion)
		{
			throw new NotImplementedException();
		}
	}
}

