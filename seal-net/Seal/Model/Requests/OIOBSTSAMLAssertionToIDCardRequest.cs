using dk.nsi.seal.Constants;
using System;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.Requests
{
    public class OIOBSTSAMLAssertionToIDCardRequest : OIOBSTSAMLAssertionRequest
	{
		public OIOBSTSAMLAssertionToIDCardRequest(XDocument document) : base(document)
		{

		}

		public override OIOBSTSAMLAssertion GetOIOBSTSAMLAssertion(XElement assertionElement)
		{
			return OIOBSTSAMLAssertionFactory.CreateOIOBSTSAMLAssertion(assertionElement);
		}

		public string GetITSystemName()
		{
			string claimValue = GetClaimValueByUri(MedComAttributes.ItSystemName);
			if (claimValue == null)
			{
				throw new ModelException("ITSystemName required in OIOBSTSAMLAssertionToIDCardRequest");
			}
			return claimValue;
		}

		public string GetUserRole()
		{
			return GetClaimValueByUri(MedComAttributes.UserRole);
		}

		public String GetUserAuthorizationCode()
		{
			return GetClaimValueByUri(MedComAttributes.UserAuthorizationCode);
		}

		public String GetSubjectNameId()
		{
			return GetClaimValueByUri(SosiAttributes.SubjectNameId);
		}

	}
}
