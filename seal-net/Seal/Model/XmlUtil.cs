using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model
{
	public static class XmlUtil
	{
		public static XElement CreateElement(ITag tag) => new XElement(tag.Ns + tag.TagName);
	}
}
