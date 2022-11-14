using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class DsTags : ITag
	{
		public static DsTags X509Certificate => new DsTags("X509Certificate");
		public static DsTags X509Data => new DsTags("X509Data");
		public static DsTags KeyInfo => new DsTags("KeyInfo");
		public static DsTags KeyName => new DsTags("KeyName");
		public static DsTags Signature => new DsTags("Signature");
		public static DsTags Reference => new DsTags("Reference");

		protected DsTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xds;
		public string TagName { get; private set; }
	}
}
