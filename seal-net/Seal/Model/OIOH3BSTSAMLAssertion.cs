using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Vault;

namespace dk.nsi.seal.Model
{
    public class OIOH3BSTSAMLAssertion : OIOBSTSAMLAssertion
	{
		public static SAMLAttribute SamlSpecVer { get; } = CreateSamlAttribute("specVer", "https://data.gov.dk/model/core/specVersion", true);
		public static SAMLAttribute SamlSpecVerAdditional { get; } = CreateSamlAttribute("specVerAdditional", "https://healthcare.data.gov.dk/model/core/specVersion", true);
		public static SAMLAttribute SamlAssuranceLevel { get; } = CreateSamlAttribute("NSIS assuranceLevel", "https://data.gov.dk/concept/core/nsis/loa", true);
		public static SAMLAttribute SamlProfessionalUuid { get; } = CreateSamlAttribute("professionalUuid", "https://data.gov.dk/model/core/eid/professional/uuid/persistent", false);
		public static SAMLAttribute SamlCvr { get; } = CreateSamlAttribute("cvr", "https://data.gov.dk/model/core/eid/professional/cvr", true);
		public static SAMLAttribute SamlOrganizationName { get; } = CreateSamlAttribute("organizationName", "https://data.gov.dk/model/core/eid/professional/orgName", true);
		public static SAMLAttribute SamlEmail { get; } = CreateSamlAttribute("email", "https://data.gov.dk/model/core/eid/email", false);
		public static SAMLAttribute SamlAlias { get; } = CreateSamlAttribute("alias", "https://data.gov.dk/model/core/eid/alias", false);
		public static SAMLAttribute SamlPrivileges { get; } = CreateSamlAttribute("privileges", "https://data.gov.dk/model/core/eid/privilegesIntermediate", false);


		public OIOH3BSTSAMLAssertion(XElement xElement) : base(xElement)
		{
			ValidateElement(dom);
		}

		private static SAMLAttribute CreateSamlAttribute(string id, string name, bool mandatory)
		{
			return new SAMLAttribute(id, name, "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", mandatory);

		}
		public override string Id { get => dom.Attribute(SamlAttributes.Id).Value; }
		public override string Issuer => dom.Descendants(SamlTags.Issuer.Ns + SamlTags.Issuer.TagName).FirstOrDefault()?.Value;

		public override string CommonName => GetAttributeValue(OioSamlAttributes.CommonName);
		public override string Cpr => null;
		public override string CvrNumberIdentifier => GetAttributeValue(OioSaml3Attributes.Cvr);
		public override string Email => GetAttributeValue(OioSaml3Attributes.Email);
		public override DateTime NotBefore => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotBefore)?.Value);
		public override DateTime NotOnOrAfter => DateTime.Parse(dom.Descendants(SamlTags.Conditions.Ns + SamlTags.Conditions.TagName).FirstOrDefault()?.Attribute(SamlAttributes.NotOnOrAfter)?.Value);
		public override string OrganizationName => GetAttributeValue(OioSamlAttributes.OrganizationName, OioSaml3Attributes.OrganizationName);
		public override string SurName => GetAttributeValue(OioSamlAttributes.Surname);
		public override string AssuranceLevel => GetAttributeValue(OioSamlAttributes.AssuranceLevel, OioSaml3Attributes.AssuranceLevel);
		public override string SpecVersion => GetAttributeValue(OioSamlAttributes.SpecVersion, OioSaml3Attributes.SpecVersion);
		public override string AudienceRestriction => dom.Descendants(SamlTags.Audience.Ns + SamlTags.Audience.TagName).FirstOrDefault()?.Value;
		public override DateTime UserAuthenticationInstant => DateTime.Parse(dom.Descendants(SamlTags.AuthnStatement.Ns + SamlTags.AuthnStatement.TagName).FirstOrDefault()?.Attribute(SamlAttributes.AuthnInstant)?.Value);
		public override string RidNumberIdentifier => GetAttributeValue(OioSamlAttributes.RidNumber);
		public override string SpecVersionAdditional => GetAttributeValue(OioSaml3Attributes.SpecVersionAdditional);
		public override BasicPrivileges BasicPrivileges { 
			get {
				var encoded = GetAttributeValue(OioSaml3Attributes.PrivilegesIntermediate);
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
		public override string Uid => GetAttributeValue(OioSaml3Attributes.ProfessionalUuid);

		public override X509Certificate2 UserCertificate => new X509Certificate2(Convert.FromBase64String(GetAttributeValue(OioSamlAttributes.UserCertificate)));

		public override Type AssertionType => Type.OIOH3BST;

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
