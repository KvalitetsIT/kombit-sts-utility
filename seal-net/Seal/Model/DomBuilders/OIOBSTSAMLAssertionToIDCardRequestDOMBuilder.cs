using dk.nsi.seal.Constants;
using dk.nsi.seal.Model.Constants;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.DomBuilders
{
    public class OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<T> : OioWsTrustRequestDomBuilder where T : OIOBSTSAMLAssertion
    {
        private AbstractOIOBSTSAMLAssertionBuilder<T> assertionBuilder;
        private OIOBSTSAMLAssertion assertion;

        public string ItSystemName { get; set; }
        public string UserRole { get; set; }
        public string UserAuthorizationCode { get; set; }
        public string SubjectNameId { get; set; }
        public PublicKey EncryptionKey { get; set; }

        [Obsolete]
        public OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<T> SetOIOSAMLAssertion(AbstractOIOBSTSAMLAssertionBuilder<T> builder)
        {
            this.assertionBuilder = builder;
            return this;
        }

        public OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<T> SetOIOSAMLAssertion(T bst)
        {
            this.assertion = bst;
            return this;
        }

        protected override void ValidateBeforeBuild()
        {
            base.ValidateBeforeBuild();
            Validate("signingVault", SigningVault);

            if (assertionBuilder != null && assertion != null)
            {
                throw new ModelException("It is not allowed to set both assertion and assertionBuilder.");
            }

            if (assertion == null)
            {
                assertion = ValidateAssertion();
            }

            Validate("ITSystemName", ItSystemName);
        }

        protected OIOBSTSAMLAssertion ValidateAssertion()
        {
            /*Validate Assertion*/
            Validate("OIOBSTSAMLAssertion", assertionBuilder);
            assertionBuilder.ValidateBeforeBuild();

            return assertionBuilder.Build();
        }

        protected void AddRootAttributes(XElement envelope)
        {
            base.AddRootAttributes(envelope);
        }

        protected override void AddExtraNamespaceDeclarations(XElement envelope) => base.AddExtraNamespaceDeclarations(envelope);

        protected override void AddActAsTokens(XElement actAs)
        {
            if (EncryptionKey != null)
            {
                var encryptedAssertion = new EncryptionUtil().EncryptAssertion(assertion.Dom, EncryptionKey);
                actAs.Add(encryptedAssertion);
            }
            else
            {
                AddOioSamlAssertion(actAs);
            }
        }

        private void AddOioSamlAssertion(XElement actAs)
        {
            AddAssertion(actAs, assertion);
        }
        protected override void AddClaims(XElement requestSecurityToken)
        {
            XElement claimsElm = XmlUtil.CreateElement(WstTags.Claims);
            claimsElm.SetAttributeValue(WsTrustAttributes.Dialect, WsfAuthValues.ClaimsDialect);
            requestSecurityToken.Add(claimsElm);

            XElement cprClaimElm = BuildClaimElement(MedComAttributes.ItSystemName, ItSystemName);
            claimsElm.Add(cprClaimElm);

            if (UserRole != null)
            {
                XElement userRoleClaimElm = BuildClaimElement(MedComAttributes.UserRole, UserRole);
                claimsElm.Add(userRoleClaimElm);
            }

            if (UserAuthorizationCode != null)
            {
                XElement userAuthorizationCodeClaimElm = BuildClaimElement(MedComAttributes.UserAuthorizationCode, UserAuthorizationCode);
                claimsElm.Add(userAuthorizationCodeClaimElm);
            }

            if (SubjectNameId != null)
            {
                XElement subjectNameIDClaimElm = BuildClaimElement(SosiAttributes.SubjectNameId, SubjectNameId);
                claimsElm.Add(subjectNameIDClaimElm);
            }
        }

        private XElement BuildClaimElement(string attributeName, string value)
        {
            XElement cprClaimElm = XmlUtil.CreateElement(WsfAuthTags.ClaimType);
            cprClaimElm.SetAttributeValue(WsfAuthAttributes.Uri, attributeName);

            XElement valueElm = XmlUtil.CreateElement(WsfAuthTags.Value);
            valueElm.Value = value;
            cprClaimElm.Add(valueElm);

            return cprClaimElm;
        }

        private void AddAssertion(XElement actAs, OIOBSTSAMLAssertion assertion)
        {
            actAs.Add(assertion.XAssertion);
        }
    }
}
