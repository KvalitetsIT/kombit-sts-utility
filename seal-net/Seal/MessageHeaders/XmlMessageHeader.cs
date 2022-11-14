using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal.MessageHeaders
{
    public class XmlMessageHeader : MessageHeader
    {
        private readonly XElement xml;

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            foreach (var elm in xml.Elements()) { elm.WriteTo(writer); }
        }

        private XmlMessageHeader(XElement xml) => this.xml = xml;

        public override string Name => "Header";

        public override string Namespace => NameSpaces.dgws;

        public override bool MustUnderstand => false;

        public static XmlMessageHeader XmlHeader(XElement xml) => new XmlMessageHeader(xml);
        public static XmlMessageHeader XmlHeader<T>(T serializable) => new XmlMessageHeader(SerializerUtil.Serialize(serializable).Root);
    }
}