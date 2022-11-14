using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class WsaTags : ITag
	{
		public static WsaTags Action => new WsaTags("Action");
		public static WsaTags Address => new WsaTags("Address");
		public static WsaTags EndpointReference => new WsaTags("EndpointReference");
		public static WsaTags RelatesTo => new WsaTags("RelatesTo");
		public static WsaTags MessageId => new WsaTags("MessageID");
		public static WsaTags To => new WsaTags("To");
		public static WsaTags Metadata => new WsaTags("Metadata");

		protected WsaTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xwsa;
		public string TagName { get; private set; }
	}
}
