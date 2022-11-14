using System.Xml.Linq;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class OIOBSTSAMLAssertionToIDCardResponseModelBuilder
    {
        public OIOBSTSAMLAssertionToIDCardResponse Build(XDocument build)
        {
            return new OIOBSTSAMLAssertionToIDCardResponse(build);
        }
    }
}
