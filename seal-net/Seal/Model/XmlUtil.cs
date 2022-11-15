using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model
{
	public static class XmlUtil
	{
        public static XElement CreateElement(ITag tag, params object[] value) => new XElement(tag.Ns + tag.TagName, value);
    }
}
