using System.Xml.Linq;
using dk.nsi.seal.Model.Requests;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class OioSamlAssertionToIdCardRequestModelBuilder
	{

		public OioSamlAssertionToIdCardRequest Build(XDocument doc)
		{
			return new OioSamlAssertionToIdCardRequest(doc);
		}
	}
}
