using System.Xml.Linq;
using dk.nsi.seal.dgwstypes;

namespace dk.nsi.seal.pki
{
    public interface ISignatureProvider
    {
        XElement Sign(Assertion ass);
    }
}
