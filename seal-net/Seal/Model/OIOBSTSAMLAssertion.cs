using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public abstract class OIOBSTSAMLAssertion : AbstractDomInfoExtractor
	{
		public enum Type
		{
			OIOH2BST, OIOH3BST, OIO3BST, OIO2BST_CITIZEN
		}

		/**
		 * Danner rammen for de enums skabt i sub-klasserne til at opbygge
		 * <AttributeStatement> strukturen i Assertion
		 */
		public class SAMLAttribute
		{
			public SAMLAttribute(string Id, string Name, string NameFormat, bool Mandatory)
			{
				this.Id = Id;
				this.Name = Name;
				this.NameFormat = NameFormat;
				this.Mandatory = Mandatory;
			}

			public string Id { get; }
			public string Name { get; }
			public string NameFormat { get; }
			public bool Mandatory { get; }

            public override bool Equals(Object o)
            {
                var other = o as SAMLAttribute;
                if (other == null)
                {
                    return false;
                }

                return this.Id.Equals(other.Id);
            }

            public override int GetHashCode()
            {
				return Id.GetHashCode();
            }
		}

		public OIOBSTSAMLAssertion(XElement element) : base(element) { }

		public XElement Dom => dom;
        public abstract string Id { get; }
		public abstract string Issuer { get; }
		public abstract string CommonName { get; }
		public abstract string Cpr { get; }
		public abstract string CvrNumberIdentifier { get; }
		public abstract string Email { get; }
		public abstract DateTime NotBefore { get; }
		public abstract DateTime NotOnOrAfter { get; }
		public abstract string OrganizationName { get; }
		public abstract string SurName { get; }
		public abstract string AssuranceLevel { get; }
		public abstract string SpecVersion { get; }
		public abstract string AudienceRestriction { get; }
		public abstract DateTime UserAuthenticationInstant { get; }

		public abstract string RidNumberIdentifier { get; }

		public abstract string SpecVersionAdditional { get; }

		public abstract string Recipient { get; }
		public abstract string SubjectNameId { get; }

		public abstract string SubjectNameIdFormat { get; }
		public abstract string Uid { get; }

		public abstract X509Certificate2 UserCertificate { get; }

		public X509Certificate2 SigningCertificate
		{
			get
			{
				var certificate = dom.Element(DsTags.Signature.Ns + DsTags.Signature.TagName)?
									.Element(DsTags.KeyInfo.Ns + DsTags.KeyInfo.TagName)?
									.Element(DsTags.X509Data.Ns + DsTags.X509Data.TagName)?
									.Element(DsTags.X509Certificate.Ns + DsTags.X509Certificate.TagName)?.Value;

				return certificate != null ? new X509Certificate2(Convert.FromBase64String(certificate)) : null;
			}
		}

		public abstract Type AssertionType { get; }

		public virtual BasicPrivileges BasicPrivileges => null;

		public string GetAttributeValue(params string[] name)
		{
			var attributes = dom.Descendants(SamlTags.Attribute.Ns + SamlTags.Attribute.TagName);
			return attributes.FirstOrDefault(element =>
			{
				var xAttribute = element.Attribute(SamlAttributes.Name);
				return xAttribute != null && name.Contains(xAttribute.Value);
			})?.Value;
		}

		public static string GetAttributeValue(XElement element, params string[] name)
		{
			var attributes = element.Descendants(SamlTags.Attribute.Ns + SamlTags.Attribute.TagName);
			return attributes.FirstOrDefault(e =>
			{
				var xAttribute = e.Attribute(SamlAttributes.Name);
				return xAttribute != null && name.Contains(xAttribute.Value);
			})?.Value;
		}

		public X509Certificate GetHolderOfKeyCertificate()
		{
			XElement subjectConfirmation = GetTag(new List<ITag>() { SamlTags.Assertion, SamlTags.Subject, SamlTags.SubjectConfirmation });
			string method = subjectConfirmation != null ? subjectConfirmation.Attribute("Method").Value : null;
			bool isHolderOfKeyMethod = SamlValues.ConfirmationMethodHolderOfKey.Equals(method);

			X509Certificate certificate = null;
			if (isHolderOfKeyMethod)
			{
				XElement certificateElm = GetTag(subjectConfirmation, new List<ITag>() { SamlTags.SubjectConfirmation, SamlTags.SubjectConfirmationData, DsTags.KeyInfo, DsTags.X509Data, DsTags.X509Certificate });
				if (certificateElm != null)
				{
					certificate = new X509Certificate(System.Convert.FromBase64String(certificateElm.Value));
				}
			}

			if (certificate == null)
			{
                throw new ModelBuildException("Could not find holder-of-key certificate");

            }
			return certificate;
		}
	}
}
