using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class OioWsTrustResponse : OioWsTrustMessage
    {
        public bool IsFault { get => GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, SoapTags.Fault }) != null; }
        public string FaultString { get => !IsFault ? null : SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Body, SoapTags.Fault, CommonTags.FaultString); }
        public string FaultActor { get => !IsFault ? null : SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Body, SoapTags.Fault, CommonTags.FaultActor); }
        public string FaultCode { get => !IsFault ? null : SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Body, SoapTags.Fault, CommonTags.FaultCode); }
        public string AppliesTo { get => SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityTokenResponseCollection, WstTags.RequestSecurityTokenResponse, WspTags.AppliesTo, WsaTags.EndpointReference, WsaTags.Address); }
        public string Context { get => SafeGetAttribute(WsTrustAttributes.Context, SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityTokenResponseCollection, WstTags.RequestSecurityTokenResponse); }
        public string RelatesTo { get => SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Header, WsaTags.RelatesTo); }
        public string TokenType { get => SafeGetTagTextContent(SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityTokenResponseCollection, WstTags.RequestSecurityTokenResponse, WstTags.TokenType); }
        public DateTime Created
        {
            get
            {
                if (IsFault)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return DateTime.Parse(SafeGetTagTextContent(new List<ITag>() {
                        SoapTags.Envelope,
                        SoapTags.Body,
                        WstTags.RequestSecurityTokenResponseCollection,
                        WstTags.RequestSecurityTokenResponse,
                        WstTags.Lifetime,
                        WsuTags.Created }));
                }
            }
        }

        public DateTime Expires
        {
            get
            {
                if (IsFault)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return DateTime.Parse(SafeGetTagTextContent(new List<ITag>() {
                        SoapTags.Envelope,
                        SoapTags.Body,
                        WstTags.RequestSecurityTokenResponseCollection,
                        WstTags.RequestSecurityTokenResponse,
                        WstTags.Lifetime,
                        WsuTags.Expires}));
                }
            }
        }

        public OioWsTrustResponse(XDocument doc) : base(doc) { }

        public void ValidateSignatureAndTrust(Federation.Federation federation)
        {
            if (!SignatureUtil.Validate(dom, federation, Configuration.CheckTrust, Configuration.CheckCrl, Configuration.CheckDate))
            {
                throw new ModelBuildException("Liberty signature could not be validated");
            }
        }
    }
}
