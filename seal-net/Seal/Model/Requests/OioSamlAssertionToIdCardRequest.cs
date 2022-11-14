using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.Requests
{
    public class OioSamlAssertionToIdCardRequest : OioWsTrustRequest
	{

		public string UserAuthorizationCode => GetAttributeValue(HealthcareSamlAttributes.UserAuthorizationCode);
		public string UserEducationCode => GetAttributeValue(HealthcareSamlAttributes.UserEducationCode);
		public string UserGivenName => GetAttributeValue(HealthcareSamlAttributes.UserGivenName);
		public string UserSurName => GetAttributeValue(HealthcareSamlAttributes.UserSurName);
		public string ItSystemName => GetAttributeValue(HealthcareSamlAttributes.ItSystemName);

		public OioSamlAssertionToIdCardRequest(XDocument doc) : base(doc) { }

        public string GetAttributeValue(string name)
        {
            var attributes = dom.Descendants(SamlTags.Attribute.Ns + SamlTags.Attribute.TagName);
            return attributes.FirstOrDefault(element =>
            {
                var xAttribute = element.Attribute(SamlAttributes.Name);
                return xAttribute != null && xAttribute.Value.Equals(name);
            })?.Value;
        }

        public OioSamlAssertion OioSamlAssertion => new OioSamlAssertion(GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken, Wst14Tags.ActAs, SamlTags.Assertion }));
	}

}
