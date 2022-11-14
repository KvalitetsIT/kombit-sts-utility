using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class XmlEncTags : ITag
	{
		public static XmlEncTags EncryptedData => new XmlEncTags("EncryptedData");

		protected XmlEncTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xxmlEnc;
		public string TagName { get; private set; }
	}
}
