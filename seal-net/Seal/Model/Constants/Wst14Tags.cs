using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class Wst14Tags : ITag
	{
		public static Wst14Tags ActAs => new Wst14Tags("ActAs");

		protected Wst14Tags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xwst14;
		public string TagName { get; private set; }

	}
}
