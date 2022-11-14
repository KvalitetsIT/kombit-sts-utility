using dk.nsi.seal.Model.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class OIO2BSTCitizenSAMLAssertion : OIOBSTSAMLAssertion
    {
        public static SAMLAttribute SamlSpecVer { get; } = CreateSamlAttribute("specVer", "dk:gov:saml:attribute:SpecVer", true);
        public static SAMLAttribute SamlAssuranceLevel { get; } = CreateSamlAttribute("assuranceLevel", "dk:gov:saml:attribute:AssuranceLevel", true);
        public static SAMLAttribute SamlCpr { get; } = CreateSamlAttribute("cpr", "dk:gov:saml:attribute:CprNumberIdentifier", true);

        public OIO2BSTCitizenSAMLAssertion(XElement xElement) : base(xElement) => ValidateElement(dom);

        public OIO2BSTCitizenSAMLAssertion(XElement xElement, bool validate) : base(xElement) { if(validate) { ValidateElement(dom); } }

		public override Type AssertionType => Type.OIO2BST_CITIZEN;
		public override string AssuranceLevel => GetAttributeValue(SamlAssuranceLevel.Name);
		public override string SpecVersion => GetAttributeValue(SamlSpecVer.Name);
		public override string Cpr => GetAttributeValue(SamlCpr.Name);

        public void ValidateSignatureAndTrust() => InternalValidateSignatureAndTrust(Configuration.CheckTrust, Configuration.CheckCrl, Configuration.CheckDate);

        public void InternalValidateSignatureAndTrust(bool checkTrust, bool checkCrl, bool checkDate)
        {
            var signatureElement = dom.Element(DsTags.Signature.Ns + DsTags.Signature.TagName);
            if (signatureElement == null)
            {
                throw new ModelException("OIOSAMLAssertion is not signed");
            }
            List<XElement> referencedSignedElements = SignatureUtil.DereferenceSignedElements(signatureElement, dom);
            if (!referencedSignedElements.Contains(dom))
            {
                throw new ModelException("OIOSAMLAssertion element is not referenced by contained signature");
            }

            if (!SignatureUtil.Validate(dom, checkTrust, checkCrl, checkDate))
            {
                throw new ModelException("Signature on OIOSAMLAssertion is invalid");
            }
        }

        private static SAMLAttribute CreateSamlAttribute(string id, string name, bool mandatory) => 
            new SAMLAttribute(id, name, "urn:oasis:names:tc:SAML:2.0:attrname-format:basic", mandatory);

        public override DateTime NotOnOrAfter => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotOnOrAfter)?.Value);

        public override string Id => dom.Attribute(SamlAttributes.Id).Value;

        public override string Issuer => dom.Descendants(SamlTags.Issuer.Ns + SamlTags.Issuer.TagName).FirstOrDefault()?.Value;

        public override string CommonName => GetAttributeValue(OioSamlAttributes.CommonName);

        public override string CvrNumberIdentifier => null;

        public override string Email => GetAttributeValue(OioSaml3Attributes.Email);

        public override DateTime NotBefore => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotBefore)?.Value);

        public override string OrganizationName => null;

        public override string SurName => GetAttributeValue(OioSamlAttributes.Surname);

        public override string AudienceRestriction => dom.Descendants(SamlTags.Audience.Ns + SamlTags.Audience.TagName).FirstOrDefault()?.Value;

        public override DateTime UserAuthenticationInstant => DateTime.Parse(dom.Descendants(SamlTags.AuthnStatement.Ns + SamlTags.AuthnStatement.TagName).FirstOrDefault()?.Attribute(SamlAttributes.AuthnInstant)?.Value);

        public override string RidNumberIdentifier => null;

        public override string SpecVersionAdditional => null;

        public override string Recipient =>
                dom.Descendants(SamlTags.SubjectConfirmationData.Ns + SamlTags.SubjectConfirmationData.TagName)
                    .FirstOrDefault()?.Attribute(SamlAttributes.Recipient)?.Value;

        public override string SubjectNameId => dom.Descendants(SamlTags.NameID.Ns + SamlTags.NameID.TagName).FirstOrDefault()?.Value;

        public override string SubjectNameIdFormat => dom.Descendants(SamlTags.NameID.Ns + SamlTags.NameID.TagName).FirstOrDefault()?.Attribute(SamlAttributes.Format)?.Value;

        public override string Uid => null;

        public override X509Certificate2 UserCertificate => throw new NotImplementedException();

        private void ValidateElement(XElement element)
        {
            if (!(NameSpaces.xsaml.Equals(element.Name.Namespace) && 
                element.Name.Equals(SamlTags.Assertion.Ns + SamlTags.Assertion.TagName)))
            {
                throw new ArgumentException("Element is not a SAML assertion");
            }
        }

        public void ValidateTimestamp()
        {
            ValidateTimestamp(0);
        }

        public void ValidateTimestamp(long allowedDriftInSeconds)
        {
            if (allowedDriftInSeconds < 0) throw new ArgumentException("'allowedDriftInSeconds' must not be negative!");
            var now = DateTimeEx.UtcNowRound;

            if (now.AddSeconds(allowedDriftInSeconds) < NotBefore)
            {
                throw new ModelException("OIOSAML token is not valid yet - now: " + now.FormatDateTimeXml() +
                        ". OIOSAML token validity start: " + NotBefore.FormatDateTimeXml() + ". Allowed clock drift: " + allowedDriftInSeconds + " seconds");
            }
            if (now.AddSeconds(-allowedDriftInSeconds) > NotOnOrAfter)
            {
                throw new ModelException("OIOSAML token no longer valid - now: " + now.FormatDateTimeXml() +
                        ". OIOSAML token validity end: " + NotOnOrAfter.FormatDateTimeXml() + ". Allowed clock drift: " + allowedDriftInSeconds + " seconds");
            }
        }
    }
}
