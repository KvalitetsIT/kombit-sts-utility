using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
	public class OioWsTrustMessage : AbstractDomInfoExtractor
	{

		public OioWsTrustMessage(XDocument doc) : base(doc)	{ }


		/// <summary>
		/// Retrieve the action part of the SOAP header.
		///
		/// <pre>
		///   &lt;soap:Header&gt;
		///     &lt;wsa:Action&gt;http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue&lt;/wsa:Action&gt;
		///     ...
		///   &lt;/soap:Header&gt;
		/// </pre>
		/// </summary>
		public string Action => SafeGetTagTextContent(new List<ITag>() { SoapTags.Envelope, SoapTags.Header, WsaTags.Action });


		/// <summary>
		/// Retrieve the MessageID part of the SOAP header.
		///
		/// <pre>
		///   &lt;soap:Header&gt;
		///     ...
		///     &lt;wsa:MessageID&gt;urn:uuid:99999777-0000-0000&lt;/wsa:MessageID&gt;
		///     ...
		///   &lt;/soap:Header&gt;
		/// </pre>
		/// </summary>
		public String MessageID => SafeGetTagTextContent(new List<ITag>() { SoapTags.Envelope, SoapTags.Header, WsaTags.MessageId });

		/// <summary>
		/// Gets the signing certificate
		/// </summary>
		/// <returns> Returns the certificate contained in the XML signature or <code>null</code> if the OIOWSTrust message was not signed</returns>
		public X509Certificate2 GetSigningCertificate()
		{
			var certificateElm = GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Header, WsseTags.Security, DsTags.Signature, DsTags.KeyInfo, DsTags.X509Data, DsTags.X509Certificate });
			if (certificateElm == null)
			{
				return null;
			}
			else
			{
				return new X509Certificate2(Convert.FromBase64String(certificateElm.Value));
			}
		}


		/// <summary>
		/// Checks the signature on the <see cref="OioWsTrustMessage"/> - no trust-checks are performed.
		/// </summary>
		public void ValidateSignature()
		{
			var signedXml = new SealSignedXml(dom);
			var envSignatureOk = signedXml.CheckEnvelopeSignature();
			var assertionSignatureOk = signedXml.CheckAssertionSignature();

			if(!assertionSignatureOk)
            {
				var assertion = dom.Element(SoapTags.Body.Ns + SoapTags.Body.TagName)?
								   .Element(WstTags.RequestSecurityTokenResponseCollection.Ns + WstTags.RequestSecurityTokenResponseCollection.TagName)?
								   .Element(WstTags.RequestSecurityTokenResponse.Ns + WstTags.RequestSecurityTokenResponse.TagName)?
								   .Element(WstTags.RequestedSecurityToken.Ns + WstTags.RequestedSecurityToken.TagName)?
								   .Element(SamlTags.Assertion.Ns + SamlTags.Assertion.TagName);

				assertionSignatureOk = assertion == null ? assertionSignatureOk : new SealSignedXml(assertion).CheckAssertionSignature();
			}
			if (!envSignatureOk || !assertionSignatureOk)
			{
				throw new ModelBuildException("Liberty signature could not be validated");
			}
		}
	}
}
