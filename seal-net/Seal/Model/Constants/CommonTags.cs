using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class CommonTags : ITag
    {
		public static CommonTags FaultString => new CommonTags("faultstring");
		public static CommonTags FaultActor => new CommonTags("faultactor");
		public static CommonTags FaultCode => new CommonTags("faultcode");

		public CommonTags(string tag)
        {
			TagName = tag;
        }

 		public XNamespace Ns => "";

        public string TagName { get; private set; }
    }
}
