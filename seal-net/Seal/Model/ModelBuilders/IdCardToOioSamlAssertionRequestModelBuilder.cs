using System.Xml.Linq;
using dk.nsi.seal.Model.Requests;

namespace dk.nsi.seal.Model.ModelBuilders
{
	public class IdCardToOioSamlAssertionRequestModelBuilder
	{
		public IdCardToOioSamlAssertionRequest Build(XDocument doc) => new IdCardToOioSamlAssertionRequest(doc);
	}
}
