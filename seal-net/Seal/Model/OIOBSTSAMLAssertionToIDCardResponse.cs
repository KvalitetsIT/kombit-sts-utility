using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using System.Collections.Generic;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class OIOBSTSAMLAssertionToIDCardResponse : OioWsTrustResponse
    {
        public OIOBSTSAMLAssertionToIDCardResponse(XDocument build) : base(build)
        {
        }

        public IdCard IdCard { get 
            {
                if(IsFault)
                {
                    return null;
                }
                else
                {
                    var assertion = GetTag(new List<ITag>()
                                                {
                                                    SoapTags.Envelope,
                                                    SoapTags.Body,
                                                    WstTags.RequestSecurityTokenResponseCollection,
                                                    WstTags.RequestSecurityTokenResponse,
                                                    WstTags.RequestedSecurityToken,
                                                    SamlTags.Assertion

                                                });

                    return assertion != null ? new IdCardModelBuilder().BuildModel(assertion) : null;
                }
            }
        }
    }
}
