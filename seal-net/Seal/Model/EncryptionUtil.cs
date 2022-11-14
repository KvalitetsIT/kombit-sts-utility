using dk.nsi.seal.Model.Constants;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
    public class EncryptionUtil
    {
        public XElement EncryptAssertion(XElement assertion, PublicKey EncryptionKey)
        {
            Aes sessionKey = Aes.Create();
            sessionKey.KeySize = 128;

            var assertionDocument = new XmlDocument();
            assertionDocument.Load(assertion.CreateReader());

            EncryptedXml eXml = new EncryptedXml();
            byte[] encryptedElement = eXml.EncryptData(assertionDocument.DocumentElement, sessionKey, false);


            EncryptedData encryptedData = new EncryptedData();
            encryptedData.Type = EncryptedXml.XmlEncElementUrl;
            encryptedData.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES128Url);

            byte[] encryptedByteKey = EncryptedXml.EncryptKey(sessionKey.Key, (RSA)EncryptionKey.Key, false);
            EncryptedKey encryptedKey = new EncryptedKey();
            encryptedKey.CipherData = new CipherData(encryptedByteKey);
            encryptedKey.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);

            encryptedData.KeyInfo.AddClause(new KeyInfoEncryptedKey(encryptedKey));

            encryptedData.CipherData.CipherValue = encryptedElement;

            XElement encryptedAssertion = XmlUtil.CreateElement(SamlTags.EncryptedAssertion);
            encryptedAssertion.Add(encryptedData.GetXml().OwnerDocument);

            using (var nodeReader = new XmlNodeReader(encryptedData.GetXml()))
            {
                nodeReader.MoveToContent();
                encryptedAssertion.Add(XDocument.Load(nodeReader).Root);
            }

            return encryptedAssertion;
        }

        public XElement DecryptAssertion(XElement encryptedAssertionElement, AsymmetricAlgorithm privateKey)
        {
            var encryptedAssertionDocument = new XmlDocument();
            encryptedAssertionDocument.Load(encryptedAssertionElement.CreateReader());

            XmlElement encryptedDataElement = GetElement(XmlEncTags.EncryptedData.TagName, XmlEncTags.EncryptedData.Ns.NamespaceName, encryptedAssertionDocument.DocumentElement);

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.LoadXml(encryptedDataElement);

            XmlNodeList keyInfoNodeList = encryptedAssertionDocument.GetElementsByTagName(DsTags.KeyInfo.TagName, DsTags.KeyInfo.Ns.NamespaceName);

            KeyInfo key = new KeyInfo();
            key.LoadXml((XmlElement)keyInfoNodeList[0]);

            SymmetricAlgorithm symmetricKey = null;
            foreach (KeyInfoClause keyInfoClause in key)
            {
                if (keyInfoClause is KeyInfoEncryptedKey keyInfoEncryptedKey)
                {
                    EncryptedKey encryptedKey = keyInfoEncryptedKey.EncryptedKey;
                    symmetricKey = new RijndaelManaged
                    {
                        Key = EncryptedXml.DecryptKey(encryptedKey.CipherData.CipherValue, (RSA)privateKey, false)
                    };

                    break;
                }
            }

            EncryptedXml encryptedXml = new EncryptedXml();
            byte[] plaintext = encryptedXml.DecryptData(encryptedData, symmetricKey);

            XmlDocument assertion = new XmlDocument();
            assertion.Load(new StringReader(Encoding.UTF8.GetString(plaintext)));

            return XElement.Load(new XmlNodeReader(assertion));
        }

        private XmlElement GetElement(string element, string elementNS, XmlElement doc)
        {
            XmlNodeList list = doc.GetElementsByTagName(element, elementNS);
            if (list.Count == 0)
            {
                return null;
            }

            return (XmlElement)list[0];
        }
    }
}
