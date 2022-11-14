using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
	public sealed class WsseTags : ITag
	{
		public static WsseTags Security => new WsseTags("Security");
		public static WsseTags SecurityTokenReference => new WsseTags("SecurityTokenReference");

		private WsseTags(string tag) => TagName = tag;

		public XNamespace Ns => NameSpaces.xwsse;

		public string TagName { get; }
	}
}
