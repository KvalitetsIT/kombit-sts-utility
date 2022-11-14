using System.IO;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal
{
    static class MessageExt
    {
        public static Stream AsStream(this MessageBuffer msgbuf)
        {
            Message tmpMessage = msgbuf.CreateMessage();

            var stream = new MemoryStream();
            using (var wr = XmlWriter.Create(stream))
            {
                tmpMessage.WriteMessage(wr);
            }
            stream.Position = 0;
            return stream;
        }

        public static Stream ToStream(this Message msg)
        {
            MessageBuffer msgbuf = msg.CreateBufferedCopy(int.MaxValue);
            Message tmpMessage = msgbuf.CreateMessage();

            var stream = new MemoryStream();
            using (var wr = XmlWriter.Create( stream) )
            {
                tmpMessage.WriteMessage(wr);
            }
            stream.Position = 0;
            return stream;
        }

        public static Message ToMessage(this XDocument xdoc, MessageVersion mv)
        {
            using (var ms = new MemoryStream())
            {
                xdoc.Save(ms, SaveOptions.DisableFormatting);
                ms.Position = 0;
                using (var reader = XmlDictionaryReader.CreateTextReader(ms, XmlDictionaryReaderQuotas.Max))
                {
                    var msgbuf = Message.CreateMessage(reader, int.MaxValue, mv).CreateBufferedCopy(int.MaxValue);
                    return msgbuf.CreateMessage();
                }
            }

        }    
    }
}