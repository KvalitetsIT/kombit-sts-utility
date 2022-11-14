using System;
using System.Runtime.Serialization;
using System.Xml;

namespace dk.nsi.seal
{
    internal class faultwriter : XmlObjectSerializer
    {
        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            throw new NotImplementedException();
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            throw new NotImplementedException();
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            var xe = graph as string;
            writer.WriteElementString("FaultCode", NameSpaces.dgws, xe);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
        }
    }
}