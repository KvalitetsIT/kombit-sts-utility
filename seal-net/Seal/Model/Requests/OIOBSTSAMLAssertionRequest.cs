using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.Requests
{
    public abstract class OIOBSTSAMLAssertionRequest : OioWsTrustRequest
	{

		public OIOBSTSAMLAssertionRequest(XDocument doc) : base(doc)
		{
			// Empty
		}

		protected XElement GetAssertionElement()
		{
			return GetTag(GetActAsElement(), new List<ITag>() { Wst14Tags.ActAs, SamlTags.Assertion });
		}


		public OIOBSTSAMLAssertion OIOBSTSAMLAssertion { get {
				XElement assertionElement = GetAssertionElement();
				return  assertionElement != null ? GetOIOBSTSAMLAssertion(assertionElement) : null;
			} 
		}


		public abstract OIOBSTSAMLAssertion GetOIOBSTSAMLAssertion(XElement assertionElement);

		public void ValidateHolderOfKeyRelation()
		{
			OIOBSTSAMLAssertion oiosamlAssertion = OIOBSTSAMLAssertion;
			if (oiosamlAssertion == null)
			{
				throw new ModelBuildException("No assertion element found in request.");
			}

			if (!GetSigningCertificate().Equals(oiosamlAssertion.GetHolderOfKeyCertificate()))
			{
				throw new ModelBuildException("Signing certificate and holder-of-key certificate relation do not match.");
			}
		}

		public Dictionary<string, string> getClaimMap()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			List<XElement> claims = base.GetClaims();
			foreach (XElement claim in claims)
			{
				String type = claim.Attribute(WsfAuthAttributes.Uri).Value;
				type = result.ContainsKey(type) ? type + "(duplicate)" : type;
				string value = claim.Value;
				result.Add(type, value);
			}

			return result;
		}

		protected String GetClaimValueByUri(String uri)
		{
			return getClaimMap()[uri];
		}
	}
}
