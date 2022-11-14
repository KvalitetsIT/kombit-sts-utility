using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal.Xml
{
    public static class XmlUtilities
    {
        public static string GetTextNodeValue(XElement element)
        {
            List<XNode> children = (List<XNode>) element.DescendantNodes();
            if (!children.Any())
            {
                throw new XmlException("The supplied element doesn't have any child nodes");
            }

            XNode child = children[0];

            //Maybe do additional checks here?
            if (child.NodeType != XmlNodeType.Text && child.NodeType != XmlNodeType.CDATA)
            {
                throw new XmlException("The first child of the supplied node is not a text element");
            }

            //?? Not sure this works
            return ((XElement)child).Value;
        }

        public static List<XElement> GetElementsByLocalNameAndNamespace(XElement domElement, string nameSpaceName, string localName)
        {
            List<XElement> elements = new List<XElement>();
            foreach (XNode node in domElement.DescendantNodes())
            {
                if (node is XElement)
                {
                    XElement element = (XElement)node;
                    if (element.Name.NamespaceName.Equals(nameSpaceName) && element.Name.LocalName.Equals(localName))
                        elements.Add(element);
                }
            }
            return elements;
        }

        public static string CreateNonce()
        {
            long now = DateTime.Now.Ticks;
            byte[] nonce = new byte[20];

            // Copy currentTimeMillis to the upper 8 bytes
            for (int i = 0; i < 8; i++)
            {
                nonce[7 - i] = (byte)(now & 0xFF);
                now = now >> 8;
            }

            // Copy 8 random bytes to the lower 8 bytes
            Array.Copy(Guid.NewGuid().ToByteArray(), 0, nonce, 8, 8);

            // Place a "magic" in the upper 4 bytes
            nonce[16] = 0x53;
            nonce[17] = 0x4F;
            nonce[18] = 0x53;
            nonce[19] = 0x49;

            return Convert.ToBase64String(nonce);
        }
    }
}
