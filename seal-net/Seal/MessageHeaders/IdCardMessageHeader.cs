using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace dk.nsi.seal.MessageHeaders
{
    public class IdCardMessageHeader : MessageHeader
    {
        public readonly IdCard idc;
        public readonly Guid id;
        public readonly DateTime createdTime;

        private IdCardMessageHeader(IdCard idc)
        {
            id = Guid.NewGuid();
            createdTime = DateTime.Now;
            this.idc = idc;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            //Id is not allowed on security element
            writer.WriteStartElement("Timestamp", NameSpaces.wsu);
            writer.WriteElementString("Created", NameSpaces.wsu, createdTime.ToString("u").Replace(' ', 'T'));
            writer.WriteEndElement();
            idc.Xassertion.WriteTo(writer);
        }

        public static IdCardMessageHeader IdCardHeader(IdCard idc) => new IdCardMessageHeader(idc);

        public override string Name => "Security";

        public override string Namespace => NameSpaces.wsse;

        public override bool MustUnderstand => false;
    }
}