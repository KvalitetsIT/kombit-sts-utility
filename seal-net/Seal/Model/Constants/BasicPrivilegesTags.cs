using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    class BasicPrivilegesTags : ITag
    {
		public static BasicPrivilegesTags PrivilegeList => new BasicPrivilegesTags("PrivilegeList");
		public static XName PrivilegeGroup => XName.Get("PrivilegeGroup");
		public static XName Privilege => XName.Get("Privilege");
		public static XName Constraint => XName.Get("Constraint");
		protected BasicPrivilegesTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xbpp;
		public string TagName { get; private set; }

    }
}
