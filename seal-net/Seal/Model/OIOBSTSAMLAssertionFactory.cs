using dk.nsi.seal.Model.Constants;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class OIOBSTSAMLAssertionFactory
    {
		public static OIOBSTSAMLAssertion CreateOIOBSTSAMLAssertion(XElement assertionElm)
		{
			string specVersion = OIOBSTSAMLAssertion.GetAttributeValue(assertionElm, OioSaml3Attributes.SpecVersionAdditional);
			if(specVersion == null)
            {
				specVersion = OIOBSTSAMLAssertion.GetAttributeValue(assertionElm, OioSaml3Attributes.SpecVersion);
			}
			if(specVersion == null)
            {
				specVersion = OIOBSTSAMLAssertion.GetAttributeValue(assertionElm, OioSamlAttributes.SpecVersion);
			}

			switch (specVersion == null ? "" : specVersion)
			{
				case "OIO-SAML-H-3.0": return new OIOH3BSTSAMLAssertion(assertionElm); 
				case "OIO-SAML-3.0": return new OIO3BSTSAMLAssertion(assertionElm);
				case "DK-SAML-2.0": return new OIOH2BSTSAMLAssertion(assertionElm); 
				default: throw new ModelException("Unknown OIOBSTSAMLAssertion specification");
			}
		}
	}
}
