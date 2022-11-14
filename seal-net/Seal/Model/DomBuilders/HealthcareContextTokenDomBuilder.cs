using System.Xml.Linq;
using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.DomBuilders
{
    public class HealthcareContextTokenDomBuilder : AbstractSamlBuilder<OioSamlAssertion>
	{
		public string SubjectNameId { get; set; }
		public string SubjectNameIdFormat { get; set; }

		public string SubjectName { get; set; }
		public string UserAuthorizationCode { get; set; }
		public string UserEducationCode { get; set; }
		public string UserGivenName { get; set; }
		public string UserSurName { get; set; }
		public string ItSystem { get; set; }


		public HealthcareContextTokenDomBuilder(string userGivenName)
		{
			this.UserGivenName = userGivenName;
		}




		public override OioSamlAssertion Build()
		{
			var assertion = CreateDocument();

			return new OioSamlAssertion(assertion);
		}

		public override void ValidateBeforeBuild()
		{
			base.ValidateBeforeBuild();
			Validate("SubjectNameId", SubjectNameId);
			Validate("SubjectNameIdFormat", SubjectNameIdFormat);
		}

		protected override void AppendToRoot(XDocument document, XElement assertion)
		{
			base.AppendToRoot(document, assertion);
			CreateSubject(assertion);
			CreateAttributeStatements(assertion);

		}

		protected override void CreateSubject(XElement assertion)
		{
			var subject = XmlUtil.CreateElement(SamlTags.Subject);

			var nameId = XmlUtil.CreateElement(SamlTags.NameID);
			nameId.Add(new XAttribute(SamlAttributes.Format, SamlValues.NameFormatUri));
			nameId.Value = SubjectName;
			subject.Add(nameId);

			var subjectConfirmation = XmlUtil.CreateElement(SamlTags.SubjectConfirmation);
			subjectConfirmation.Add(new XAttribute(SamlAttributes.Method, SamlValues.ConfirmationMethodSenderVouches));
			var subjectConfirmationData = XmlUtil.CreateElement(SamlTags.SubjectConfirmationData);
			subjectConfirmation.Add(subjectConfirmationData);
			subject.Add(subjectConfirmation);

			assertion.Add(subject);
		}

		protected void CreateAttributeStatements(XElement assertion)
		{
			var attributeStatement = XmlUtil.CreateElement(SamlTags.AttributeStatement);
			CreateAttributeStatement(attributeStatement, HealthcareSamlAttributes.UserAuthorizationCode, UserAuthorizationCode, "", SubjectNameIdFormat);
			CreateAttributeStatement(attributeStatement, HealthcareSamlAttributes.UserEducationCode, UserEducationCode, "", SubjectNameIdFormat);
			CreateAttributeStatement(attributeStatement, HealthcareSamlAttributes.UserGivenName, UserGivenName, "", SubjectNameIdFormat);
			CreateAttributeStatement(attributeStatement, HealthcareSamlAttributes.UserSurName, UserSurName, "", SubjectNameIdFormat);
			CreateAttributeStatement(attributeStatement, HealthcareSamlAttributes.ItSystemName, ItSystem, "", SubjectNameIdFormat);
			assertion.Add(attributeStatement);
		}

		protected void CreateAttributeStatement(XElement attributeStatement, string name, string value, string friendlyName, string nameFormat)
		{
			if (value != null)
			{
				attributeStatement.Add(CreateAttributeElement(name, value, friendlyName, nameFormat));
			}

		}

	}
}
