using dk.nsi.seal.Constants;
using System;
using System.Xml.Linq;
using static dk.nsi.seal.Model.OIOBSTSAMLAssertion;

namespace dk.nsi.seal.Model
{
    public class OIO2BSTCitizenSAMLAssertionBuilder : AbstractOIOBSTSAMLAssertionBuilder<OIO2BSTCitizenSAMLAssertion>
	{
		public OIO2BSTCitizenSAMLAssertionBuilder()
		{
			AddAttribute(OIO2BSTCitizenSAMLAssertion.SamlSpecVer, "DK-SAML-2.0");
		}
		
		protected override SAMLAttribute[] GetAttributes()
		{
			return new SAMLAttribute[] {
				OIO2BSTCitizenSAMLAssertion.SamlSpecVer,
				OIO2BSTCitizenSAMLAssertion.SamlAssuranceLevel,
				OIO2BSTCitizenSAMLAssertion.SamlCpr,
			 };
		}

		public void SetAssuranceLevel(string level)
		{
			AddAttribute(OIO2BSTCitizenSAMLAssertion.SamlAssuranceLevel, level);
		}

		protected override string NameIdFormat => SamlValues.NameidFormatPersistent;

		public string Cpr { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlCpr, value); }

		protected override OIO2BSTCitizenSAMLAssertion CreateAssertion(XElement elm)
		{
			return new OIO2BSTCitizenSAMLAssertion(elm);
		}

		protected override void CreateSubject(XElement assertion)
		{
			throw new NotImplementedException();
		}
	}
}
