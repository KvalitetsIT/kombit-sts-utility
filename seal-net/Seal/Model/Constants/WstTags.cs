using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class WstTags : ITag
	{
		public static WstTags RequestSecurityToken => new WstTags("RequestSecurityToken");
		public static WstTags RequestSecurityTokenResponseCollection => new WstTags("RequestSecurityTokenResponseCollection");
		public static WstTags RequestSecurityTokenResponse => new WstTags("RequestSecurityTokenResponse");
		public static WstTags Lifetime => new WstTags("Lifetime");
		public static WstTags TokenType => new WstTags("TokenType");
		public static WstTags RequestedSecurityToken => new WstTags("RequestedSecurityToken");
		public static WstTags Claims => new WstTags("Claims");

		protected WstTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xwst13;
		public string TagName { get; private set; }

	}
}
