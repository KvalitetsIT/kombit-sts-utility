using System;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Vault;

namespace dk.nsi.seal.Model.DomBuilders
{
    public abstract class OioWsTrustDomBuilder : AbstractSoapBuilder
    {
        public ICredentialVault SigningVault { get; set; }

        public string Action { get; set; } = WsTrustConstants.Wst13IssueAction;

        protected abstract void AddExtraHeaders(XElement header);

        protected abstract void AddExtraNamespaceDeclarations(XElement envelope);

        protected override void AddHeaderContent(XElement header)
        {
            var action = XmlUtil.CreateElement(WsaTags.Action);
            action.Add(new XAttribute("mustUnderstand", "1"));
            action.Add(new XAttribute(NameSpaces.xwsu + "Id", "action"));
            header.Add(action);
            action.Value = Action;

            var messageId = XmlUtil.CreateElement(WsaTags.MessageId);
            messageId.Add(new XAttribute(NameSpaces.xwsu + "Id", "messageID"));
            header.Add(messageId);
            messageId.Value = "urn:uuid:" + Guid.NewGuid().ToString("D");

            AddExtraHeaders(header);
            var security = AddWsSecurityHeader(header);
            AddWsuTimestamp(security);
        }

        protected override void AddRootAttributes(XElement envelope)
        {
            AddExtraNamespaceDeclarations(envelope);
        }

        /// <summary>
        /// Build the final response <code>Document</code>.<br />
        /// Before the<code>Document</code> is generated all attributes will be validated.<br />
        /// <br />
        /// A<code> Document</code> is generated each time this method is called.Calling this method multiple times will therefore return multiple objects.
        /// </summary>
        /// <returns></returns>
        public override XDocument Build()
        {
            var document = CreateDocument();
            SealUtilities.CheckAndSetSamlDsPreFix(document);
            NameSpaces.SetMissingNamespaces(document);
            if (SigningVault != null)
            {
                var signer = new SealSignedXml(document);
                var signedXml = signer.Sign(SigningVault.GetSystemCredentials());
                var xDocument = XDocument.Parse(signedXml.OuterXml, LoadOptions.PreserveWhitespace);

                return xDocument;
            }
            return document;
        }

        protected void AddSecurityTokenReferenceToIdCard(XElement parent)
        {
            var securityTokenReference = XmlUtil.CreateElement(WsseTags.SecurityTokenReference);
            parent.Add(securityTokenReference);
            var reference = new XElement(NameSpaces.xwsse + "Reference", new XAttribute("URI", "#IDCard"));
            securityTokenReference.Add(reference);
        }

        private void AddWsuTimestamp(XElement securityHeader)
        {
            var timestamp = XmlUtil.CreateElement(WsuTags.Timestamp);
            timestamp.Add(new XAttribute(NameSpaces.xwsu + "Id", "timestamp"));
            securityHeader.Add(timestamp);
            var created = XmlUtil.CreateElement(WsuTags.Created);
            created.Value = DateTimeEx.UtcNowRound.FormatDateTimeXml();
            timestamp.Add(created);
        }

        private XElement AddWsSecurityHeader(XElement header)
        {
            var securityHeader = XmlUtil.CreateElement(WsseTags.Security);
            securityHeader.Add(new XAttribute(WsseAttributes.MustUnderstand, "1"));
            header.Add(securityHeader);
            return securityHeader;
        }
    }
}
