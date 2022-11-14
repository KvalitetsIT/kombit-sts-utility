using System.Xml.Linq;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class OioSamlAssertionToIdCardResponseModelBuilder
    {
        public OioSamlAssertionToIdCardResponse Build(XDocument doc) => new OioSamlAssertionToIdCardResponse(doc);
    }
}