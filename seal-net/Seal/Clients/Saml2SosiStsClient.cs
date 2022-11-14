using dk.nsi.seal.Factories;
using dk.nsi.seal.Federation;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Model.ModelBuilders;
using dk.nsi.seal.Vault;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal
{
    public static class Saml2SosiStsClient
    {
        public static IdCard ExchangeAssertion(X509Certificate2 clientCertificate, Uri uri, OioSamlAssertion assertion, string authorizationCode)
        {
            var request = new OioSamlAssertionToIdCardRequestDomBuilder
            {
                OioSamlAssertion = assertion,
                Audience = "urn:uuid:" + Guid.NewGuid().ToString("D"),
                SigningVault = new InMemoryCredentialVault(clientCertificate),
                ItSystemName = "Sygdom.dk",
                UserAuthorizationCode = authorizationCode
            }
            .Build();

            var response = SendRequest(uri, request);
            response.ValidateSignature();
            var federation = new SosiTestFederation(new CrlCertificateStatusChecker());
            response.ValidateSignatureAndTrust(federation);

            return response.IdCard();
        }

        private static OioSamlAssertionToIdCardResponse SendRequest(Uri uri, XDocument xDocument)
        {
            var response = WebPost(xDocument.Root, uri);
            var modelBuilder = OIOSAMLFactory.CreateOioSamlAssertionToIDCardResponseModelBuilder();
            return modelBuilder.Build(new XDocument(response));
        }

        private static XElement WebPost(XElement request, Uri uri)
        {
            try
            {
                var WebRequest = System.Net.WebRequest.Create(uri) as HttpWebRequest;
                WebRequest.Method = "POST";
                WebRequest.ContentType = "text/xml; charset=utf-8";
                WebRequest.Headers.Add("SOAPAction", "http://sosi.org/webservices/sts/1.0/stsService/RequestSecurityToken");
                using (var ms = new MemoryStream())
                {
                    var w = new XmlTextWriter(ms, Encoding.UTF8);
                    request.Save(w);
                    w.Flush();

                    WebRequest.ContentLength = ms.Length;
                    ms.Position = 0;
                    ms.CopyTo(WebRequest.GetRequestStream());
                }

                var response = WebRequest.GetResponse();

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