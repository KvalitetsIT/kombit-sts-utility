using System.Xml;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Model;

namespace dk.nsi.seal
{
    public class Sosi2SamlStsClient
    {
        public Sosi2SamlStsClient() { }

        const string action = "http://sosi.dk/ibo#Issue";

        public static OioWsTrustResponse ExchangeAssertion(Uri uri, string audience, IdCard idc)
        {
            var request = new IdCardToOioSamlAssertionRequestDomBuilder
            {
                IdCardAssertion = SerializerUtil.Serialize(idc.GetAssertion<Assertion>()).Root,
                Audience = audience,
                PlaceIdCardInSoapHeader = true,
                Action = action
            };

            return new OioWsTrustResponse(new XDocument(ExchangeAssertion(request, uri)));
        }

        public static OioWsTrustResponse ExchangeAssertion(X509Certificate2 clientCertificate, Uri uri, string appliesTo, IdCard idc)
        {
            var request = new IdCardToOioSamlAssertionRequestDomBuilder
            {
                IdCardAssertion = SerializerUtil.Serialize(idc.GetAssertion<Assertion>()).Root,
                Audience = appliesTo,
                PlaceIdCardInSoapHeader = true,
                Action = action,
                SigningVault = new InMemoryCredentialVault(clientCertificate)
            };

            return new OioWsTrustResponse(new XDocument(ExchangeAssertion(request, uri)));
        }

        private static XElement ExchangeAssertion(IdCardToOioSamlAssertionRequestDomBuilder request, Uri uri) => WebPost(uri, request.Build().Root);

        private static XElement WebPost(Uri uri, XElement request)
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                webRequest.Method = "POST";
                webRequest.ContentType = "text/xml; charset=utf-8";
                webRequest.Headers.Add("SOAPAction", action);
                using (var ms = new MemoryStream())
                {
                    var w = new XmlTextWriter(ms, Encoding.UTF8);
                    request.Save(w);
                    w.Flush();

                    webRequest.ContentLength = ms.Length;
                    ms.Position = 0;
                    ms.CopyTo(webRequest.GetRequestStream());
                }
                var response = webRequest.GetResponse();

                return XElement.Load(response.GetResponseStream(), LoadOptions.PreserveWhitespace);
            }
            catch (WebException ex)
            {
                if (ex.Response == null) throw;
                return XElement.Load(ex.Response.GetResponseStream());
            }
        }
    }
}