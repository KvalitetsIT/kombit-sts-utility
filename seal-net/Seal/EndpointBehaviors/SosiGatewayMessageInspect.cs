using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml.Linq;

namespace dk.nsi.seal
{
    class SosiGatewayMessageInspect : IClientMessageInspector
    {
        public string To;
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var h = request.Headers;
            var idx = request.Headers.FindHeader("Action", "http://schemas.microsoft.com/ws/2005/05/addressing/none");
            if (idx != -1)
            {
                var action = request.Headers.GetHeader<string>(idx);
                request.Headers.RemoveAt(idx);
                request.Headers.Add(MessageHeader.CreateHeader("Action", "http://www.w3.org/2005/08/addressing", action));
            }
            request.Headers.Add(MessageHeader.CreateHeader("To", "http://www.w3.org/2005/08/addressing", To));

            return null;
        }
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                MessageBuffer msgbuf = reply.CreateBufferedCopy(int.MaxValue);
                Message tmpMessage = msgbuf.CreateMessage();

                var xdoc = new XDocument();
                using (var wr = xdoc.CreateWriter())
                {
                    tmpMessage.WriteBody(wr);
                }

                var q = from h in xdoc.Root.Elements(NameSpaces.xsoap + "Header")
                    from i in h.Elements(NameSpaces.xsosi + "implicitLoginHeader")
                    from r in i.Elements(NameSpaces.xsosi + "requestIdCardDigestForSigningResponse")
                    select new SosiGWLoginError()
                    {
                        DigestValue = r.Element(NameSpaces.xds + "DigestValue").Value,
                        BrowserUrl = r.Element(NameSpaces.xsosi + "BrowserUrl").Value
                    };

                var le = q.FirstOrDefault();
                var reason = xdoc.Root.Elements(NameSpaces.xsoap + "Body")
                    .Elements(NameSpaces.xsoap + "Fault")
                    .Elements("faultstring")
                    .Select(v => v.Value).FirstOrDefault();
                if (le != null)
                    throw new FaultException<SosiGWLoginError>(le, new FaultReason(reason));

                var dgwsfc = xdoc.Root.Descendants(NameSpaces.xdgws + "FaultCode").FirstOrDefault();
                if (dgwsfc != null)
                {
                    var fs = xdoc.Root.Descendants("faultstring").First();
                    throw new FaultException<XElement>(dgwsfc, new FaultReason(fs.Value), new FaultCode("Server"), "");
                }
                reply = msgbuf.CreateMessage();
            }
        }
    }
}