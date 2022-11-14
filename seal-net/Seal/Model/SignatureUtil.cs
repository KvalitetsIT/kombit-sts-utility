using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Federation;

namespace dk.nsi.seal.Model
{
    public static class SignatureUtil
    {
        public static bool Validate(XElement signatureToValidate, bool checkTrustForTrustedCertificates, bool checkRevoked, bool checkDate) =>
            Validate(signatureToValidate, null, checkTrustForTrustedCertificates, checkRevoked, checkDate);

        public static bool Validate(XElement signatureToValidate, Federation.Federation federation, bool checkTrustForTrustedCertificates, bool checkRevoked, bool checkDate)
        {
            if (signatureToValidate.NodeType != XmlNodeType.Element)
            {
                throw new ModelException("The signature to validate must be a ds:Signature Element!");
            }

            var xml = new XmlDocument();
            xml.Load(signatureToValidate.CreateReader());

            bool isAssertion = false;
            var nsManager = NameSpaces.MakeNsManager(xml.NameTable);
            var sig = xml.SelectSingleNode("/soap:Envelope/soap:Header/wsse:Security/ds:Signature", nsManager) as XmlElement;
            if (sig == null)
            {
                sig = xml.SelectSingleNode("/saml:Assertion/ds:Signature", nsManager) as XmlElement;
                isAssertion = true;
                if (sig == null)
                {
                    sig = xml.GetElementsByTagName("Signature", NameSpaces.ds)[0] as XmlElement;
                    isAssertion = true;
                }
            }
            if (sig == null) return false;
            var signature = new Signature();
            sig = MakeSignatureCheckSamlCompliant(sig);
            signature.LoadXml(sig);
            var certificate = signature.KeyInfo.Cast<KeyInfoX509Data>().Select(d => d.Certificates[0] as X509Certificate2).FirstOrDefault(c => c != null);

            if (checkDate && !CheckDates(certificate)) { return false; }

            //Check that the certificate used for validation is trusted. If a Federation has been specified
            //the signature must have been created by the STS. If no federation is specified, the
            //certificate must be trusted in the certificate store of the device the code is executed on.
            if (checkTrustForTrustedCertificates)
            {
                var trusted = false;
                if (federation != null) { trusted = federation.IsValidSTSCertificate(certificate); }
                else { trusted = certificate.Verify(); }

                if (!trusted) { throw new ModelException("The certificate that signed the security token is not trusted!"); }
            }
            // check the certificates CRL if the certificate is revoked
            if (checkRevoked)
            {
                CrlCertificateStatusChecker crlChecker = new CrlCertificateStatusChecker();
                var isValid = crlChecker.GetRevocationStatus(certificate).IsValid;
                if (!isValid)
                {
                    throw new ModelException("The certificate or one in its certificate chain has been revoked!");
                }
            }

            // check if xml is actually signed with key sent in message
            var signed = new SealSignedXml(signatureToValidate);
            if (isAssertion)
            {
                return signed.CheckAssertionSignature();
            }
            return signed.CheckEnvelopeSignature();
        }

        private static XmlElement MakeSignatureCheckSamlCompliant(XmlElement sig)
        {
            if (sig.Attributes.GetNamedItem("Id") is XmlAttribute)
            {
                return sig;
            }

            if (sig.Attributes.GetNamedItem("id") is XmlAttribute)
            {
                var oldId = sig.Attributes.GetNamedItem("id") as XmlAttribute;
                sig.Attributes.Remove(oldId);
                var newId = sig.OwnerDocument.CreateAttribute("Id");
                newId.Value = oldId.Value;
                sig.Attributes.Append(newId);
            }

            return sig;
        }
        private static bool CheckDates(X509Certificate2 cert)
        {
            return cert.NotBefore < DateTime.Now && cert.NotAfter > DateTime.Now;
        }

        public static List<XElement> DereferenceSignedElements(XElement signatureElement, XElement dom)
        {
            var references = signatureElement.Descendants(DsTags.Reference.Ns + DsTags.Reference.TagName).ToList();
            var assertions = dom.DescendantsAndSelf(SamlTags.Assertion.Ns + SamlTags.Assertion.TagName).ToList();
            var elements = new List<XElement>();
            foreach (var reference in references)
            {
                var uri = reference.Attribute(DsAttributes.Uri).Value.Substring(1);
                var element = assertions.FirstOrDefault(xElement =>
                {
                    var firstOrDefault = xElement.Attributes(SamlAttributes.Id).FirstOrDefault() ??
                                         xElement.Attributes(SamlAttributes.Id.ToLower()).FirstOrDefault() ??
                                         xElement.Attributes("Id").FirstOrDefault();

                    return firstOrDefault != null && firstOrDefault.Value.Equals(uri);
                });
                if (element != null)
                {
                    elements.Add(element);
                }
            }
            return elements;
        }
    }

}
