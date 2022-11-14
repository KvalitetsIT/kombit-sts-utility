using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class SoapTags : ITag
	{

		public static SoapTags Body => new SoapTags("Body");
		public static SoapTags Header => new SoapTags("Header");
		public static SoapTags Fault => new SoapTags("Fault");
		public static SoapTags Envelope => new SoapTags("Envelope");

		protected SoapTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xsoap;
		public string TagName { get; private set; }
	}
}
