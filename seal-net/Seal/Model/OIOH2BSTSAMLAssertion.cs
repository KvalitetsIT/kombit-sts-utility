using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Vault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class OIOH2BSTSAMLAssertion : OIOBSTSAMLAssertion
    {
        public static SAMLAttribute SamlSpecVer { get; } = CreateSamlAttribute("specVer", "dk:gov:saml:attribute:SpecVer", true);
        public static SAMLAttribute SamlAssuranceLevel { get; } = CreateSamlAttribute("NIST assuranceLevel", "dk:gov:saml:attribute:AssuranceLevel", true);
        public static SAMLAttribute SamlCpr { get; } = CreateSamlAttribute("cpr", "dk:gov:saml:attribute:CprNumberIdentifier", true);
        public static SAMLAttribute SamlRid { get; } = CreateSamlAttribute("rid", "dk:gov:saml:attribute:RidNumberIdentifier", false);
        public static SAMLAttribute SamlCvr { get; } = CreateSamlAttribute("cvr", "dk:gov:saml:attribute:CvrNumberIdentifier", true);
        public static SAMLAttribute SamlOrganizationName { get; } = CreateSamlAttribute("organizationName", "urn:oid:2.5.4.10", true);
        public static SAMLAttribute SamlProfessionalUuid { get; } = new SAMLAttribute("professionalUuid", "https://data.gov.dk/model/core/eid/professional/uuid/persistent", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", false);
        public static SAMLAttribute SamlPrivileges { get; } = new SAMLAttribute("privileges", "dk:gov:saml:attribute:Privileges_intermediate", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", false);

        public OIOH2BSTSAMLAssertion(XElement xElement) : base(xElement)
        {
            ValidateElement(dom);
        }
        private static SAMLAttribute CreateSamlAttribute(string id, string name, bool mandatory)
        {
            return new SAMLAttribute(id, name, "urn:oasis:names:tc:SAML:2.0:attrname-format:basic", mandatory);

        }
        public override string Id { get => dom.Attribute(SamlAttributes.Id).Value; }
        public override string Issuer => dom.Descendants(SamlTags.Issuer.Ns + SamlTags.Issuer.TagName).FirstOrDefault()?.Value;

        public override string CommonName => GetAttributeValue(OioSamlAttributes.CommonName);
        public override string Cpr => GetAttributeValue(SamlCpr.Name);
        public override string CvrNumberIdentifier => GetAttributeValue(SamlCvr.Name);
        public override string Email => GetAttributeValue(OioSaml3Attributes.Email);
        public override DateTime NotBefore => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotBefore)?.Value);
        public override DateTime NotOnOrAfter => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotOnOrAfter)?.Value);
        public override string OrganizationName => GetAttributeValue(SamlOrganizationName.Name);
        public override string SurName => GetAttributeValue(OioSamlAttributes.Surname);
        public override string AssuranceLevel => GetAttributeValue(SamlAssuranceLevel.Name);
        public override string SpecVersion => GetAttributeValue(SamlSpecVer.Name);
        public override string AudienceRestriction => dom.Descendants(SamlTags.Audience.Ns + SamlTags.Audience.TagName).FirstOrDefault()?.Value;
        public override DateTime UserAuthenticationInstant => DateTime.Parse(dom.Descendants(SamlTags.AuthnStatement.Ns + SamlTags.AuthnStatement.TagName).FirstOrDefault()?.Attribute(SamlAttributes.AuthnInstant)?.Value);
        public override string RidNumberIdentifier => GetAttributeValue(SamlRid.Name);
        public override string SpecVersionAdditional => GetAttributeValue(OioSaml3Attributes.SpecVersionAdditional);
        public override BasicPrivileges BasicPrivileges
        {
            get
            {
                var encoded = GetAttributeValue(SamlPrivileges.Name);
                return encoded == null ? null : BasicPrivileges.Decode(encoded);
            }
        }


        public override string Recipient
            =>
                dom.Descendants(SamlTags.SubjectConfirmationData.Ns + SamlTags.SubjectConfirmationData.TagName)
                    .FirstOrDefault()?.Attribute(SamlAttributes.Recipient)?.Value;
        public override string SubjectNameId => dom.Descendants(SamlTags.NameID.Ns + SamlTags.NameID.TagName).FirstOrDefault()?.Value;

        public override string SubjectNameIdFormat
            =>
                dom.Descendants(SamlTags.NameID.Ns + SamlTags.NameID.TagName)
                    .FirstOrDefault()?.Attribute(SamlAttributes.Format)?.Value;
        public override string Uid => GetAttributeValue(SamlProfessionalUuid.Name);

        public override X509Certificate2 UserCertificate => new X509Certificate2(Convert.FromBase64String(GetAttributeValue(OioSamlAttributes.UserCertificate)));

        public override Type AssertionType => Type.OIOH2BST;

        private void ValidateElement(XElement element)
        {
            if (!(NameSpaces.xsaml.Equals(element.Name.Namespace)
                    && element.Name.Equals(SamlTags.Assertion.Ns + SamlTags.Assertion.TagName)))
            {
                throw new ArgumentException("Element is not a SAML assertion");
            }
        }

        /// <summary>
        /// Checks the signature on the <see cref="OioSamlAssertion"/>.
        /// </summary>
        /// <param name="vault">The <see cref="ICredentialVault"/> containing trusted certificates used to check trust for the <see cref="OioSamlAssertion"/>.</param>
        public void ValidateSignatureAndTrust()
        {

            var signatureElement = dom.Element(DsTags.Signature.Ns + DsTags.Signature.TagName);//dom.XPathSelectElement("/" + );
            if (signatureElement == null)
            {
                throw new ModelException("OIOSAMLAssertion is not signed");
            }
            List<XElement> referencedSignedElements = SignatureUtil.DereferenceSignedElements(signatureElement, dom);
            if (!referencedSignedElements.Contains(dom))
            {
                throw new ModelException("OIOSAMLAssertion element is not referenced by contained signature");
            }

            if (!SignatureUtil.Validate(dom, Configuration.CheckTrust, Configuration.CheckCrl, Configuration.CheckDate))
            {
                throw new ModelException("Signature on OIOSAMLAssertion is invalid");
            }
        }

        public void Sign(ICredentialVault signingVault)
        {
            var signer = new SealSignedXml(XAssertion);
            var signedXml = signer.SignAssertion(signingVault.GetSystemCredentials(), XAssertion.Attribute(SamlAttributes.Id).Value);
            dom = XElement.Parse(signedXml.OuterXml, LoadOptions.PreserveWhitespace);
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
