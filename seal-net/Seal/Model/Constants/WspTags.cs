using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class WspTags : ITag
	{
		public static WspTags AppliesTo => new WspTags("AppliesTo");

		protected WspTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xwsp;
		public string TagName { get; private set; }
	}
}
