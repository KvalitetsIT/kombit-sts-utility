using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
	public interface ITag
	{
		XNamespace Ns { get; }
		string TagName { get; }
	}
}
