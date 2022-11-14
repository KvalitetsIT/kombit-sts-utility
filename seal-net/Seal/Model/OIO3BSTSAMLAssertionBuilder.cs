using dk.nsi.seal.Constants;
using System;
using System.Xml.Linq;
using static dk.nsi.seal.Model.OIOBSTSAMLAssertion;

namespace dk.nsi.seal.Model
{
    public class OIO3BSTSAMLAssertionBuilder : AbstractOIOBSTSAMLAssertionBuilder<OIO3BSTSAMLAssertion>
	{
		public OIO3BSTSAMLAssertionBuilder()
		{
			AddAttribute(OIO3BSTSAMLAssertion.SamlSpecVer, "OIO-SAML-3.0");
		}

		protected override string NameIdFormat => SamlValues.NameidFormatPersistent;

		protected override SAMLAttribute[] GetAttributes()
		{
			return new SAMLAttribute[] {
				OIO3BSTSAMLAssertion.SamlSpecVer,
				OIO3BSTSAMLAssertion.SamlNsisAssuranceLevel,
				OIO3BSTSAMLAssertion.SamlNistAssuranceLevel,
				OIO3BSTSAMLAssertion.SamlCpr,
				OIO3BSTSAMLAssertion.SamlProfessionalUuid,
				OIO3BSTSAMLAssertion.SamlCvr,
				OIO3BSTSAMLAssertion.SamlOrganizationName,
				OIO3BSTSAMLAssertion.SamlEmail,
				OIO3BSTSAMLAssertion.SamlAlias,
				OIO3BSTSAMLAssertion.SamlPrivileges
			 };
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

		public string Uuid { set => AddAttribute(OIO3BSTSAMLAssertion.SamlProfessionalUuid, value); }
		public string Cvr { set => AddAttribute(OIO3BSTSAMLAssertion.SamlCvr, value); }
		public string Cpr { set => AddAttribute(OIO3BSTSAMLAssertion.SamlCpr, value); }
		public string OrganizationName { set => AddAttribute(OIO3BSTSAMLAssertion.SamlOrganizationName, value); }

		protected override OIO3BSTSAMLAssertion CreateAssertion(XElement elm)
		{
			return new OIO3BSTSAMLAssertion(elm);
		}

		protected override void CreateSubject(XElement assertion)
		{
			throw new NotImplementedException();
		}

	}
}

