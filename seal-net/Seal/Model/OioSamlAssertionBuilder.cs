using System;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.DomBuilders;

namespace dk.nsi.seal.Model
{
	public class OioSamlAssertionBuilder : AbstractOioSamlTokenBuilder
	{
		public CertificateInfo CertificateInfo { get; private set; }
		public string RecipientUrl { get; set; }
		public DateTime DeliveryNotOnOrAfter { get; set; }
		public string RidNumber { get; private set; }
		public bool IncludeIdCardAsBootstrapToken { get; set; }
		public X509Certificate2 UserCertificate { get; set; }

		public override void ValidateBeforeBuild()
		{
			base.ValidateBeforeBuild();
			if (!CertificateInfo.IsProbableCertificateInfoString(UserIdCard.AlternativeIdentifier))
			{
				throw new ModelException("Subject NameID is not in CertificateInfo format");
			}
			CertificateInfo = CertificateInfo.FromString(UserIdCard.AlternativeIdentifier);
			ValidateCertificateInfo();
			RidNumber = CertificateInfo.RidNumber;
			Validate("recipientURL", RecipientUrl);
			Validate("deliveryNotOnOrAfter", DeliveryNotOnOrAfter);
		}

		public override OioSamlAssertion Build()
		{
			ValidateBeforeBuild();

			XElement assertion = CreateDocument();

            if (IncludeIdCardAsBootstrapToken)
            {
                AddIdCardAsBootstrapToken(assertion);
            }

            var signer = new SealSignedXml(assertion);
			var signedXml = signer.SignAssertion(SigningVault.GetSystemCredentials(), AssertionId);
			var signedXelement = XElement.Parse(signedXml.OuterXml, LoadOptions.PreserveWhitespace);

			return new OioSamlAssertion(signedXelement);
		}

		protected override void CreateSubject(XElement assertion)
		{
			var subject = XmlUtil.CreateElement(SamlTags.Subject);

			var nameId = XmlUtil.CreateElement(SamlTags.NameID);
			nameId.Add(new XAttribute(SamlAttributes.Format, SamlValues.NameFormatUri));
			nameId.Value = CertificateInfo.SubjectDn.Name;
			subject.Add(nameId);

			var subjectConfirmation = XmlUtil.CreateElement(SamlTags.SubjectConfirmation);
			subjectConfirmation.Add(new XAttribute(SamlAttributes.Method, SamlValues.ConfirmationMethodBearer));
			var subjectConfirmationData = XmlUtil.CreateElement(SamlTags.SubjectConfirmationData);
			subjectConfirmationData.Add(new XAttribute(SamlAttributes.NotOnOrAfter, DeliveryNotOnOrAfter.FormatDateTimeXml()));
			subjectConfirmationData.Add(new XAttribute(SamlAttributes.Recipient, RecipientUrl));
			subjectConfirmation.Add(subjectConfirmationData);
			subject.Add(subjectConfirmation);

			assertion.Add(subject);
		}

		protected override void AddExtraAttributes(XElement attributeStatement)
		{
			attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.CertificateIssuer,
				CertificateInfo.IssuerDn.Name));

            attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.Uid,
				CertificateInfo.SubjectSerialNumber, OioSamlAttributes.UidFriendly));

            attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.RidNumber, RidNumber));

            attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.CertificateSerial,
				CertificateInfo.CertificateSerial, OioSamlAttributes.UidFriendly));

            attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.IsYouthCert, bool.FalseString));

			attributeStatement.Add(CreateAttributeElement(
				OioSamlAttributes.UserCertificate, 
				Convert.ToBase64String(UserCertificate.GetRawCertData())));

			attributeStatement.Add(CreateAttributeElement(
				OioSamlAttributes.CvrNumber, 
				UserIdCard.SystemInfo.CareProvider.Id, 
				"", 
				SamlValues.NameFormatBasic));

            attributeStatement.Add(CreateAttributeElement(OioSamlAttributes.OrganizationName, 
				UserIdCard.SystemInfo.CareProvider.OrgName, 
				"", 
				SamlValues.NameFormatBasic));
        }

		private void AddIdCardAsBootstrapToken(XElement assertion)
		{
			var attributeStatementElm = assertion.Element(SamlTags.AttributeStatement.Ns + SamlTags.AttributeStatement.TagName);
			var attributeElm = XmlUtil.CreateElement(SamlTags.Attribute);
			attributeElm.Add(new XAttribute(SamlAttributes.Name, OioSamlAttributes.DiscoveryEpr));
			attributeStatementElm.Add(attributeElm);

			var attributeValue = XmlUtil.CreateElement(SamlTags.AttributeValue);
			attributeElm.Add(attributeValue);

			var endpointReferenceElm = XmlUtil.CreateElement(WsaTags.EndpointReference);
			attributeValue.Add(endpointReferenceElm);

			var addressElm = XmlUtil.CreateElement(WsaTags.Address);
			addressElm.Value = LibertyValues.SosiSTSUri;
			endpointReferenceElm.Add(addressElm);

			var metadataElm = XmlUtil.CreateElement(WsaTags.Metadata);
			endpointReferenceElm.Add(metadataElm);

			var abstractElm = XmlUtil.CreateElement(LibertyDiscoveryTags.Abstract);
			abstractElm.Value = "A SOSI idcard";
			metadataElm.Add(abstractElm);

			var serviceTypeElm = XmlUtil.CreateElement(LibertyDiscoveryTags.ServiceType);
			serviceTypeElm.Value = LibertyValues.SosiUrn;
			metadataElm.Add(serviceTypeElm);

			var providerIDElm = XmlUtil.CreateElement(LibertyDiscoveryTags.ProviderId);
			providerIDElm.Value = LibertyValues.SosiSTSUri;
			metadataElm.Add(providerIDElm);

			var securityContextElm = XmlUtil.CreateElement(LibertyDiscoveryTags.SecurityContext);
			metadataElm.Add(securityContextElm);

			var securityMechID = XmlUtil.CreateElement(LibertyDiscoveryTags.SecurityMechID);
			securityMechID.Value = LibertyValues.SamlSecurityMechId;
			securityContextElm.Add(securityMechID);

			var tokenElm = XmlUtil.CreateElement(LibertySecurityTags.Token);
			tokenElm.Add(new XAttribute(LibertyAttributes.Usage, LibertyValues.TokenUsageSecurityToken));
			securityContextElm.Add(tokenElm);

			var idCardElm = UserIdCard.Xassertion;
			tokenElm.Add(idCardElm);

			var samlAttributeStatement = new Saml2AttributeStatement();
			samlAttributeStatement.Attributes.Add(new Saml2Attribute(OioSamlAttributes.DiscoveryEpr,
				endpointReferenceElm.ToString(SaveOptions.None)) {NameFormat = new Uri(SamlValues.NameFormatUri)});

		}


		protected override void AddExtraAuthnAttributes(XElement assertion)
		{
			var authnStatement = assertion.Descendants(SamlTags.AuthnStatement.Ns + SamlTags.AuthnStatement.TagName).FirstOrDefault();
			authnStatement.Add(new XAttribute(SamlAttributes.SessionIndex, AssertionId));
		}

		private void ValidateCertificateInfo()
		{
			ValidateDn(CertificateInfo.SubjectDn, "SubjectDN", true);
			ValidateDn(CertificateInfo.IssuerDn, "IssuerDN", false);
		}

		private void ValidateDn(X500DistinguishedName name, string identifier, bool requireSerial)
		{
			var cPattern = @"C=(?<c>.+)(?<!\\)";
			var cMatch = Regex.Match(name.Name, cPattern);
			Validate("Country ('C') in " + identifier, cMatch.Groups[1].Value);

			var oPattern = @"O=(?<o>.+)(?<!\\),";
			var oMatch = Regex.Match(name.Name, oPattern);
			Validate("Organization ('O') in " + identifier, oMatch.Groups[1].Value);

			var cnPattern = @"CN=(?<cn>.+)(?<!\\),";
			var cnMatch = Regex.Match(name.Name, cnPattern);
			Validate("CommonName ('CN') in " + identifier, cnMatch.Groups[1].Value);

			if (requireSerial) { Validate("SERIALNUMBER ('SERIALNUMBER') in " + identifier, CertificateInfo.GetSeialFromSubjectDn(name)); }
			
		}
	}
}
