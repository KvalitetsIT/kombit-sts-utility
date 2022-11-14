using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class LibertySecurityTags : ITag
	{
		public static LibertySecurityTags Token => new LibertySecurityTags("Token");

		protected LibertySecurityTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xLibertySecuritySchema;
		public string TagName { get; private set; }
	}
}
