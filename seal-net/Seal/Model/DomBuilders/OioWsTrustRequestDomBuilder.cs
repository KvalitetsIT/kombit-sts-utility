using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.DomBuilders
{
    public abstract class OioWsTrustRequestDomBuilder : OioWsTrustDomBuilder
    {

        /// <summary>
        /// <b>Mandatory</b>: Set the <code>Audience</code> for the requested token.<br />
        /// Example:
        /// <pre>
        ///       &lt;soap:Body&gt;
        ///          &lt;wst:RequestSecurityToken Context = "urn:uuid:00000…" & gt;
        ///              ...
        ///              &lt;wsp:AppliesTo&gt;
        ///                  &lt;wsa:EndpointReference&gt;
        ///                      &lt;wsa:Address&gt;http://fmk-online.dk&lt;/wsa:Address&gt;
        ///                  &lt;/wsa:EndpointReference&gt;
        ///              &lt;/wsp:AppliesTo&gt;
        ///          &lt;/wst:RequestSecurityToken&gt;
        ///      &lt;/soap:Body&gt;
        /// </pre>
        /// </summary>
        public string Audience { get; set; }


        /// <summary>
        /// <b>Optional</b>: Set the WS-Addressing TO element denoting the STS endpoint.
        /// </summary>
        public string WsAddressingTo { get; set; }

        protected override void ValidateBeforeBuild()
        {
            Validate("audience", Audience);

            ValidateValue("wsAddressingTo", WsAddressingTo);
        }

        protected override void AddExtraNamespaceDeclarations(XElement envelope)
        {
        }

        protected override void AddExtraHeaders(XElement header)
        {
            if (!string.IsNullOrEmpty(WsAddressingTo))
            {
                var to = new XElement(NameSpaces.xwsa + "To");
                header.Add(to);
                to.Value = WsAddressingTo;
            }
        }

        protected override void AddBodyContent(XElement body)
        {
            var requestSecurityToken = AddTokenRequest(body);
            AddActAs(requestSecurityToken);
            AddAudience(requestSecurityToken);
            AddClaims(requestSecurityToken);
        }

        private XElement AddTokenRequest(XElement body)
        {
            var xrst = new XElement(NameSpaces.xtrust + "RequestSecurityToken",
                new XAttribute("Context", Audience),
                new XElement(NameSpaces.xtrust + "TokenType", "urn:oasis:names:tc:SAML:2.0:assertion"),
                new XElement(NameSpaces.xtrust + "RequestType", "http://schemas.xmlsoap.org/ws/2005/02/security/trust/Issue")
                );
            body.Add(xrst);
            return xrst;
        }

        private void AddActAs(XElement requestSecurityToken)
        {
            var actAs = XmlUtil.CreateElement(Wst14Tags.ActAs);
            requestSecurityToken.Add(actAs);
            AddActAsTokens(actAs);
        }

        protected abstract void AddActAsTokens(XElement actAs);

        private void AddAudience(XElement parent)
        {
            var appliesTo = XmlUtil.CreateElement(WspTags.AppliesTo);
            parent.Add(appliesTo);

            var endpointReference = XmlUtil.CreateElement(WsaTags.EndpointReference);
            appliesTo.Add(endpointReference);

            var address = XmlUtil.CreateElement(WsaTags.Address);
            endpointReference.Add(address);
            address.Value = Audience;
        }

        protected virtual void AddClaims(XElement requestSecurityToken)
        {

        }
    }
}
