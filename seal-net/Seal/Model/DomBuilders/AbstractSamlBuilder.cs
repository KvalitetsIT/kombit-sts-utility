using System;
using System.Xml.Linq;
using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.DomBuilders
{
    public abstract class AbstractSamlBuilder<T>
	{

		/// <summary>
		/// <b>Mandatory</b>: Set the issuer part of the message.<br />
		/// Example:
		///
		/// <pre>
		///  &lt;saml:Assertion... &gt;
		///   &lt;saml:Issuer&gt;http://pan.certifikat.dk/sts/services/SecurityTokenService&lt;/saml:Issuer&gt;
		///   ...
		///  &lt;/saml:Assertion... &gt;
		/// </pre>
		/// </summary>
		public string Issuer { get; set; }
		public string AssertionId { get; private set; }

		public abstract T Build();

		protected abstract void CreateSubject(XElement assertion);

		protected virtual void AppendToRoot(XDocument document, XElement assertion)
		{
			var issuer = XmlUtil.CreateElement(SamlTags.Issuer);
			issuer.Value = Issuer;
			assertion.Add(issuer);
		}


		protected XElement CreateDocument()
		{
			ValidateBeforeBuild();


			var assertion = XmlUtil.CreateElement(SamlTags.Assertion);
			AddRootAttributes(assertion);
			AppendToRoot(null, assertion);

			return assertion;
		}

		protected virtual void AddRootAttributes(XElement assertion)
		{
			AssertionId = "_" + Guid.NewGuid();
			assertion.Add(new XAttribute(SamlAttributes.Id, AssertionId));
			assertion.Add(new XAttribute(SamlAttributes.IssueInstant, DateTimeEx.UtcNowRound.FormatDateTimeXml()));
			assertion.Add(new XAttribute(SamlAttributes.Version, SamlValues.SamlVersion));
		}

		protected XElement CreateAttributeElement(string name, string value, string friendlyName = "", string nameFormat = "")
		{
			var attribute = XmlUtil.CreateElement(SamlTags.Attribute);
			attribute.Add(new XAttribute(SamlAttributes.Name, name));
			if (!string.IsNullOrEmpty(friendlyName))
			{
				attribute.Add(new XAttribute(SamlAttributes.FriendlyName, friendlyName));
			}

			if (!string.IsNullOrEmpty(nameFormat))
			{
				attribute.Add(new XAttribute(SamlAttributes.NameFormat, nameFormat));
			}

			var attributeValue = XmlUtil.CreateElement(SamlTags.AttributeValue);
			if (!string.IsNullOrEmpty(value))
			{
				attributeValue.Value = value;
			}
			attribute.Add(attributeValue);

			return attribute;
		}


		public virtual void ValidateBeforeBuild()
		{
			Validate("Issuer", Issuer);
		}

		public void Validate(string attribute, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ModelException(attribute + " is mandatory - but was an empty String.");
			}
		}

		public void Validate(string attribute, object value)
		{
			if (value == null)
			{
				throw new ModelException(attribute + " is mandatory - but was null Value.");
			}
		}



	}
}
