using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.DomBuilders;
using System;
using System.Text;
using System.Xml.Linq;
using static dk.nsi.seal.Model.OIOBSTSAMLAssertion;

namespace dk.nsi.seal.Model
{
    public class OIOH2BSTSAMLAssertionBuilder : AbstractOIOBSTSAMLAssertionBuilder<OIOH2BSTSAMLAssertion>
	{
		public OIOH2BSTSAMLAssertionBuilder()
		{
			AddAttribute(OIOH2BSTSAMLAssertion.SamlSpecVer, "DK-SAML-2.0");
		}

		protected override string NameIdFormat => SamlValues.NameidFormatUnspecified;

		public BasicPrivilegesDOMBuilder Privileges { get; set; }

		protected override SAMLAttribute[] GetAttributes()
		{
			return new SAMLAttribute[] {
				OIOH2BSTSAMLAssertion.SamlSpecVer,
				OIOH2BSTSAMLAssertion.SamlAssuranceLevel,
				OIOH2BSTSAMLAssertion.SamlCpr,
				OIOH2BSTSAMLAssertion.SamlRid,
				OIOH2BSTSAMLAssertion.SamlCvr,
				OIOH2BSTSAMLAssertion.SamlOrganizationName,
				OIOH2BSTSAMLAssertion.SamlProfessionalUuid,
				OIOH2BSTSAMLAssertion.SamlPrivileges,
			 };
		}

		public override void ValidateBeforeBuild()
		{
			if (Privileges != null)
			{
				Privileges.PublicValidateBeforeBuild();
				AddPrivilegeAttribute();
			}
			base.ValidateBeforeBuild();

			/*OIOH2BST kan indeholde enten en RID eller en global UUID, men ikke begge samtidigt*/
			string rid = GetAttribute(OIOH2BSTSAMLAssertion.SamlRid);
			string uuid = GetAttribute(OIOH2BSTSAMLAssertion.SamlProfessionalUuid);
			if (rid != null && uuid != null)
			{
				throw new ModelException("Only one of RID or UUID can be present.");
			}
			else if (rid == null && uuid == null)
			{
				throw new ModelException("One of RID or UUID must be present.");
			}
		}

		private void AddPrivilegeAttribute()
		{
			var priviliges = Privileges.Build();
			string privilegesB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(priviliges.ToString()));
			AddAttribute(OIOH2BSTSAMLAssertion.SamlPrivileges, privilegesB64);
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

		public string Rid { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlRid, value); }
		public string Uuid { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlProfessionalUuid, value); }
		public string Cvr { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlCvr, value); }
		public string Cpr { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlCpr, value); }
		public string OrganizationName { set => AddAttribute(OIOH2BSTSAMLAssertion.SamlOrganizationName, value); }


		protected override OIOH2BSTSAMLAssertion CreateAssertion(XElement elm)
		{
			return new OIOH2BSTSAMLAssertion(elm);
		}
		protected override void CreateSubject(XElement assertion)
		{
			throw new NotImplementedException();
		}

	}

}
