using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using dk.nsi.seal.Model.ModelBuilders;

namespace dk.nsi.seal
{
    public class SealSignedXml : SignedXml
    {
        public XmlDocument xml { get; private set; }

        public SealSignedXml(string xml)
            : this(stringToXml(xml))
        {
        }

        public SealSignedXml(Stream stream)
            : this(streamToXml(stream))
        {
        }

        public SealSignedXml(XDocument xml)
            : this(streamToXml(XDocToStream(xml)))
        {
        }

        public SealSignedXml(XElement xml)
            : this(streamToXml(XDocToStream(xml)))
        {
        }

        public SealSignedXml(XmlDocument xml)
            : base(xml)
        {
            this.xml = xml;
        }

        private static XmlDocument stringToXml(string xml)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);
            return doc;
        }

        private static XmlDocument streamToXml(Stream stream)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(stream);
            return doc;
        }

        private static Stream XDocToStream(XDocument xml)
        {
            var ms = new MemoryStream();
            xml.Save(ms, SaveOptions.DisableFormatting);
            ms.Position = 0;
            return ms;
        }

        private static Stream XDocToStream(XElement xml)
        {
            var ms = new MemoryStream();
            xml.Save(ms, SaveOptions.DisableFormatting);
            ms.Position = 0;
            return ms;
        }

        public bool CheckEnvelopeSignature()
        {
            var nsManager = NameSpaces.MakeNsManager(xml.NameTable);
            var sig = xml.SelectSingleNode("/soap:Envelope/soap:Header/wsse:Security/ds:Signature", nsManager) as XmlElement;
            if (sig == null) throw new ModelBuildException("Could not find Liberty signature element");
	        sig = MakeSignatureCheckSamlCompliant(sig);
            LoadXml(sig);
            var cert = KeyInfo.Cast<KeyInfoX509Data>().Select(d => d.Certificates[0] as X509Certificate2).Where(c => c != null).FirstOrDefault();
            if (cert == null) throw new InvalidOperationException("No X509Certificate2 certificate found in Keyinfo");
            return CheckSignature(cert, true);
        }

        public bool CheckAssertionSignature()
        {
			var xmlass = xml.DocumentElement.LocalName == "Assertion" ? xml.DocumentElement : xml.GetElementsByTagName("Assertion", NameSpaces.saml)[0] as XmlElement;
			if (xmlass == null) throw new ModelBuildException("Could not find Liberty signature element");
			var sig = xmlass.GetElementsByTagName("Signature", NameSpaces.ds)[0] as XmlElement;
			if (sig == null) throw new ModelBuildException("Could not find Liberty signature element");
	        sig = MakeSignatureCheckSamlCompliant(sig);
	        LoadXml(sig);
            var cert = KeyInfo.Cast<KeyInfoX509Data>().Select(d => d.Certificates[0] as X509Certificate2).Where(c => c != null).FirstOrDefault();
            if (cert == null) throw new InvalidOperationException("No X509Certificate2 certificate found in Keyinfo");
            return CheckSignature(cert, true);
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

	    public X509Certificate2 GetSignature()
        {
            var nsManager = NameSpaces.MakeNsManager(xml.NameTable);
            var xmlass = xml.DocumentElement.LocalName == "Assertion" ? xml.DocumentElement : xml.GetElementsByTagName("Assertion", NameSpaces.saml)[0] as XmlElement;
            var sig = xmlass.GetElementsByTagName("Signature", NameSpaces.ds)[0] as XmlElement;
            if (sig == null) return null;
	        sig = MakeSignatureCheckSamlCompliant(sig);
            LoadXml(sig);
            var cert = KeyInfo.Cast<KeyInfoX509Data>().Select(d => d.Certificates[0] as X509Certificate2).Where(c => c != null).FirstOrDefault();
            return cert;
        }

        public static XDocument Sign(X509Certificate2 cert, XDocument doc) => XDocument.Parse(new SealSignedXml(doc).Sign(cert).OuterXml, LoadOptions.PreserveWhitespace);

        public XmlDocument Sign(X509Certificate2 cert)
        {
            var refnames = new [] { "#messageID", "#action", "#timestamp", "#body" };

            foreach (var s in refnames)
            {
                var reference = new Reference();
                reference.Uri = s;
                reference.AddTransform(new XmlDsigExcC14NTransform());
				reference.DigestMethod = XmlDsigSHA1Url;
				AddReference(reference);
            }
            try { SigningKey = cert.PrivateKey; }
            catch { SigningKey = cert.GetECDsaPrivateKey(); }
            SignedInfo.CanonicalizationMethod = new XmlDsigExcC14NTransform().Algorithm;
			SignedInfo.SignatureMethod = XmlDsigRSASHA1Url;
			KeyInfo = new KeyInfo();
            KeyInfo.AddClause(new KeyInfoX509Data(cert));

			ComputeSignature();

			XmlElement signaelm = GetXml();
			var xSecurity = xml.SelectSingleNode("/soap:Envelope/soap:Header/wsse:Security", NameSpaces.MakeNsManager(xml.NameTable)) as XmlElement;
			if (xSecurity == null) throw new InvalidOperationException("No Signature element found in /Envolope/Header/Security");
			xSecurity.AppendChild(xSecurity.OwnerDocument.ImportNode(signaelm, true));

			return xml;
        }

		public XmlDocument SignAssertion(X509Certificate2 cert, string id)
		{
			var refnames = new[] { "#" + id };
			foreach (var s in refnames)
			{
				var reference = new Reference();
				reference.Uri = s;
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                reference.AddTransform(new XmlDsigExcC14NTransform());
				reference.DigestMethod = XmlDsigSHA1Url;
				AddReference(reference);
			}

			SigningKey = cert.PrivateKey;
            SignedInfo.CanonicalizationMethod = XmlDsigExcC14NTransformUrl ; 
			SignedInfo.SignatureMethod = XmlDsigRSASHA1Url;
			KeyInfo = new KeyInfo();
			KeyInfo.AddClause(new KeyInfoX509Data(cert));

            ComputeSignature();

            XmlElement signaelm = GetXml();
            var assertion = xml.SelectSingleNode("/saml:Assertion", NameSpaces.MakeNsManager(xml.NameTable)) as XmlElement;
            if (assertion == null) throw new InvalidOperationException("No Signature element found in /Envolope/Header/Security");
            var issuer = assertion.GetElementsByTagName("Issuer").Item(0);
            assertion.InsertAfter(signaelm, issuer);

            return xml;
		}

		public XmlElement GetDGWSSign(X509Certificate2 cert)
        {
            var refnames = new [] { "#IDCard" };
            foreach (var s in refnames)
            {
                var reference = new Reference();
                reference.Uri = s;
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                reference.AddTransform(new XmlDsigExcC14NTransform());
				reference.DigestMethod = XmlDsigSHA1Url;
				AddReference(reference);
            }

            SigningKey = cert.PrivateKey;
            SignedInfo.CanonicalizationMethod = new XmlDsigExcC14NTransform().Algorithm;
			SignedInfo.SignatureMethod = XmlDsigRSASHA1Url;
			KeyInfo = new KeyInfo();
            KeyInfo.AddClause(new KeyInfoX509Data(cert));

            ComputeSignature();
            return GetXml();
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            var idElem = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", NameSpaces.MakeNsManager(doc.NameTable)) as XmlElement;
            var tid = idElem ?? base.GetIdElement(doc, id);
            return tid;
        }
    }
}
