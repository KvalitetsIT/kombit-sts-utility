using dk.nsi.seal.Model.Constants;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class OioSamlAssertionToIdCardResponse : OioWsTrustResponse
    {
        public OioSamlAssertionToIdCardResponse(XDocument doc) : base(doc) { }

        public IdCard IdCard()
        {
            if (IsFault) return null;
            var assertion = GetTag(SoapTags.Envelope,
                                        SoapTags.Body,
                                        WstTags.RequestSecurityTokenResponseCollection,
                                        WstTags.RequestSecurityTokenResponse,
                                        WstTags.RequestedSecurityToken,
                                        SamlTags.Assertion);
            return assertion != null  
                             ? new IdCardModelBuilder().BuildModel(assertion) 
                             : null;
        }
    }
}