using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;

namespace dk.nsi.seal.Model.Requests
{
    public class OioWsTrustRequest : OioWsTrustMessage
    {
        public OioWsTrustRequest(XDocument doc) : base(doc)
        {
        }

        public string AppliesTo => SafeGetTagTextContent(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken, WspTags.AppliesTo, WsaTags.EndpointReference, WsaTags.Address });

        public string Context
        {
            get
            {
                var ac = GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken });
                return ac.Attributes(WsTrustAttributes.Context).FirstOrDefault()?.Value;
            }
        }

        protected XElement GetActAsElement()
        {
            return GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken, Wst14Tags.ActAs });
        }

        protected List<XElement> GetClaims()
        {
            return GetTags(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken, WstTags.Claims, WsfAuthTags.ClaimType });
        }
        /// <summary>
        /// Checks the signature on the <see cref="OioWsTrustRequest"/> and whether the signing certificate is trusted.
        /// </summary>
        /// <param name="vault">The CredentialVault containing trusted certificates used to check trust for the <see cref="OioWsTrustRequest"/>.</param>
        public void ValidateSignatureAndTrust() => 
            InternalValidateSignatureAndTrust(Configuration.CheckTrust, Configuration.CheckCrl, Configuration.CheckDate);

        public void InternalValidateSignatureAndTrust(bool checkTrust, bool checkCrl, bool checkDate)
        {
            if (!SignatureUtil.Validate(dom, checkTrust, checkCrl, checkDate))
            {
                throw new ModelBuildException("Liberty signature could not be validated");
            }
        }
    }
}
