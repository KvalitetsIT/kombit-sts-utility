using dk.nsi.seal.Model.Requests;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class OIOBSTSAMLAssertionToIDCardRequestModelBuilder
    {
        public OIOBSTSAMLAssertionToIDCardRequest Build(XDocument doc)
        {
            return new OIOBSTSAMLAssertionToIDCardRequest(doc);
        }
    }
}
