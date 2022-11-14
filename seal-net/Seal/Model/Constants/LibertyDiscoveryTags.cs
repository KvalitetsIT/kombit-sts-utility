using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class LibertyDiscoveryTags : ITag
	{
		public static LibertyDiscoveryTags ServiceType => new LibertyDiscoveryTags("ServiceType");
		public static LibertyDiscoveryTags ProviderId => new LibertyDiscoveryTags("ProviderID");
		public static LibertyDiscoveryTags SecurityContext => new LibertyDiscoveryTags("SecurityContext");
		public static LibertyDiscoveryTags SecurityMechID => new LibertyDiscoveryTags("SecurityMechID");
		public static LibertyDiscoveryTags Abstract => new LibertyDiscoveryTags("Abstract");

		protected LibertyDiscoveryTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xLibertyDiscoverySchema;
		public string TagName { get; private set; }
	}
}
