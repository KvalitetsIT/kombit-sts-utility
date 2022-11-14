using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using static dk.nsi.seal.Model.OIOBSTSAMLAssertion;

namespace dk.nsi.seal.Model
{
    public abstract class AbstractOIOBSTSAMLAssertionBuilder<T> : AbstractSamlBuilder<T> where T : OIOBSTSAMLAssertion
    {
        private readonly IDictionary<SAMLAttribute, string> Attributes = new Dictionary<SAMLAttribute, string>();

        public string NameId { get; set; }

        public String Audience { get; set; }

        public DateTime NotOnOrAfter { get; set; }

        public ICredentialVault SigningVault { get; set; }

        public X509Certificate2 HolderOfKeyCertificate { get; set; }

        protected abstract string NameIdFormat { get; }

        public override void ValidateBeforeBuild()
        {
            Validate("nameId", NameId);
            Validate("audience", Audience);
            Validate("notOnOrAfter", NotOnOrAfter);
            Validate("holderOfCertificate", HolderOfKeyCertificate);
            Validate("signingVault", SigningVault);

            SAMLAttribute[] validAttributes = GetAttributes();
            foreach (SAMLAttribute validAttribute in validAttributes)
            {
                if (validAttribute.Mandatory)
                {
                    Validate("attribute:" + validAttribute.Name, GetAttribute(validAttribute));
                }
            }
            base.ValidateBeforeBuild();
        }

        protected abstract SAMLAttribute[] GetAttributes();

        protected override void AppendToRoot(XDocument doc, XElement root)
        {
            base.AppendToRoot(doc, root);
            XNode subjectNode = CreateSubject();
            root.Add(subjectNode);
            root.Add(CreateConditions());
            root.Add(CreateAttributeStatement(doc));
        }

        protected XNode CreateSubject()
        {
            XElement subjectElm = XmlUtil.CreateElement(SamlTags.Subject);

            XElement nameIdElm = XmlUtil.CreateElement(SamlTags.NameID);
            nameIdElm.Add(new XAttribute(SamlAttributes.Format, NameIdFormat));
            nameIdElm.Value = NameId;
            subjectElm.Add(nameIdElm);

            XElement subjectConfirmationElm = XmlUtil.CreateElement(SamlTags.SubjectConfirmation);
            subjectConfirmationElm.Add(new XAttribute(SamlAttributes.Method, SamlValues.ConfirmationMethodHolderOfKey));
            subjectElm.Add(subjectConfirmationElm);

            XElement subjectConfirmationDataElm = XmlUtil.CreateElement(SamlTags.SubjectConfirmationData);
            subjectConfirmationDataElm.Add(new XAttribute(NameSpaces.xschemaInstance + "type", "KeyInfoConfirmationDataType"));
            
            subjectConfirmationElm.Add(subjectConfirmationDataElm);

            XElement keyInfoElm = XmlUtil.CreateElement(DsTags.KeyInfo);
            subjectConfirmationDataElm.Add(keyInfoElm);

            XElement x509DataElm = XmlUtil.CreateElement(DsTags.X509Data);
            keyInfoElm.Add(x509DataElm);

            XElement x509CertificateElm = XmlUtil.CreateElement(DsTags.X509Certificate);
            String encodedCert = Convert.ToBase64String(HolderOfKeyCertificate.GetRawCertData());
            x509CertificateElm.Value = encodedCert;
            x509DataElm.Add(x509CertificateElm);

            return subjectElm;
        }


        protected XNode CreateConditions()
        {
            XElement conditionsElm = XmlUtil.CreateElement(SamlTags.Conditions);
            conditionsElm.Add(new XAttribute(SamlAttributes.NotOnOrAfter, NotOnOrAfter.FormatDateTimeXml()));

            XElement audienceRestrictionElm = XmlUtil.CreateElement(SamlTags.AudienceRestriction);
            conditionsElm.Add(audienceRestrictionElm);

            XElement audienceElm = XmlUtil.CreateElement(SamlTags.Audience);
            audienceElm.SetValue(Audience);
            audienceRestrictionElm.Add(audienceElm);

            return conditionsElm;
        }

        protected XElement CreateAttributeStatement(XDocument doc)
        {
            XElement attributeStatementElm = XmlUtil.CreateElement(SamlTags.AttributeStatement);

            ICollection<SAMLAttribute> keys = Attributes.Keys;
            foreach (SAMLAttribute key in keys)
            {
                XElement attributeElm = CreateAttributeElement(key, Attributes[key]);
                attributeStatementElm.Add(attributeElm);
            }

            return attributeStatementElm;
        }

        protected XElement CreateAttributeElement(SAMLAttribute attribute, String value)
        {
            return base.CreateAttributeElement(attribute.Name, value, nameFormat: attribute.NameFormat);
        }

        public override T Build()
        {
            XElement assertion = CreateDocument();

            var signer = new SealSignedXml(assertion);
            var signedXml = signer.SignAssertion(SigningVault.GetSystemCredentials(), AssertionId);
            var signedXelement = XElement.Parse(signedXml.OuterXml, LoadOptions.PreserveWhitespace);

            return CreateAssertion(signedXelement);
        }

        protected abstract T CreateAssertion(XElement elm);


        protected void RemoveAttribute(SAMLAttribute attribute)
        {
            this.Attributes.Remove(attribute);
        }

        protected void AddAttribute(SAMLAttribute attribute, Object obj)
        {
            string value = obj?.ToString();
            if(!this.Attributes.ContainsKey(attribute))
            {
                this.Attributes.Add(attribute, value);
            }
        }

        protected string GetAttribute(SAMLAttribute key)
        {
            return Attributes.ContainsKey(key) ? Attributes[key] : null;
        }
    }
}
