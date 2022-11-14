using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using SealTest.AssertionTests.AssertionBuilders;
using GW = SealTest.SosiGW;
using invokeResponse = SealTest.NSTWsProvider.invokeResponse;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using dk.nsi.seal.EndpointBehaviors;
using static SealTest.SosiGW.SosiGWFacadeClient.EndpointConfiguration;
using static dk.nsi.seal.MessageHeaders.XmlMessageHeader;
using static dk.nsi.seal.MessageHeaders.IdCardMessageHeader;

namespace SealTest
{
    [TestFixture]
    public class SosiGwTest
    {
        private readonly ICredentialVault moces2Vault = CredentialVaultTestUtil.GetCredentialVault();
        private readonly ICredentialVault moces3Vault = CredentialVaultTestUtil.GetMoces3CredentialVault();
        private readonly ICredentialVault voces3Vault = CredentialVaultTestUtil.GetVoces3CredentialVault();

        /// <summary>
        /// Pre-Condition: User does not have an IdCard in the GW-cache. 
        /// 
        /// Calls requestIdCardForSigning and signIdCard operations. 
        /// 
        /// The newly created idcard is stored in SOSI-GW and can be used to call services through SOSI GW. In this example the test-service (NTS) is called. 
        /// </summary>
        [Test]
        public async Task TestSosiGwRequestIdCardForSigningAndSignIdCardOces2()
        {
            var idCard = await DoLogin(moces2Vault.GetSystemCredentials(), "30808460");
            await CallNts(idCard);
        }

        /// <summary>
        /// Pre-Condition: User does not have an IdCard in the GW-cache. 
        /// 
        /// Calls requestIdCardForSigning and signIdCard operations. 
        /// 
        /// The newly created idcard is stored in SOSI-GW and can be used to call services through SOSI GW. In this example the test-service (NTS) is called. 
        /// </summary>
        [Test]
        public async Task TestSosiGwRequestIdCardForSigningAndSignIdCardOces3()
        {
            var idCard = await DoLogin(moces3Vault.GetSystemCredentials(), "91026150", "0306894781", "KT2Z4");
            await CallNts(idCard);
        }

        /// <summary>
        /// Pre-Condition: User does not have an IdCard in the GW-cache. 
        /// Input: Valid Boot Strap Token (BST). In this case it is a OIOH3BST token. 
        /// Output: Newly created and unsigned idcard.
        /// 
        /// The newly created idcard is stored in SOSI-GW and can be used to call services through SOSI GW. In this example the test-service (NTS) is called. 
        /// </summary>
        [Test]
        public Task TestSosiGwCreateIdCardFromBST()
        {
            var assertionBuilderOIOH3Bst = OIOBSTAssertionBuilderHelper.CreateOIOH3BSTSAMLAssertionBuilder(moces3Vault, voces3Vault);
            OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion> oioh3RequestBuilder = OIOBSTAssertionBuilderHelper.CreateOIOH3RequestBuilderFromAssertionBuilder(moces3Vault);
            oioh3RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIOH3Bst.Build());
            oioh3RequestBuilder.UserAuthorizationCode = "KT2Z4"; // Findes i AuthorizationServiceStub for cpr 1802602810

            var response = SendRequestSosiGw(oioh3RequestBuilder.Build());
            var idCard = response.IdCard;
            Assert.False(response.IsFault, response.FaultString);
            Assert.Null(response.GetSigningCertificate());

            return CallNts(idCard);
        }

        private static Task<invokeResponse> CallNts(IdCard idCard)
        {
            var binding = new CustomBinding();
            binding.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap11WSAddressingAugust2004, Encoding.UTF8));
            binding.Elements.Add(new HttpTransportBindingElement());

            var client = new NSTWsProvider.NtsWSProviderClient(binding, new EndpointAddress("https://test1-cnsp.ekstern-test.nspop.dk:8443/nts/service"));

            client.Endpoint.EndpointBehaviors.Add(new SealEndpointBehavior());
            client.Endpoint.EndpointBehaviors.Add(new ViaBehavior(new Uri("http://test1.ekstern-test.nspop.dk:8080/sosigw/proxy/soap-request")));

            var dgwsHeader = new Header()
            {
                SecurityLevel = 4,
                SecurityLevelSpecified = true,
                Linking = new Linking { MessageID = Guid.NewGuid().ToString("D") }
            };

            using (new OperationContextScope(client.InnerChannel))
            {
                // Adding seal-security and dgws-header soap header
                OperationContext.Current.OutgoingMessageHeaders.Add(IdCardHeader(idCard));
                OperationContext.Current.OutgoingMessageHeaders.Add(XmlHeader(dgwsHeader));

                // Throws Exception if not succesful. 
                return client.invokeAsync("test");
            }
        }

        private OIOBSTSAMLAssertionToIDCardResponse SendRequestSosiGw(XDocument xDocument)
        {
            var response = WebPost(xDocument.Root, TestConstants.SosiGwCreateIdCardFromBST);

            var modelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder();

            return modelBuilder.Build(new XDocument(response));
        }

        private XElement WebPost(XElement request, string url)
        {
            try
            {
                var WebRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
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

        public static async Task<IdCard> DoLogin(X509Certificate2 cert, string cvr, string cpr = "1802602810", string authorization = "ZXCVB")
        {
            var gwClient = new GW.SosiGWFacadeClient(SosiGWSoapBinding, "http://test1.ekstern-test.nspop.dk:8080/sosigw/service/sosigw");
            var sec = MakeSecurity(MakeAssertionForSTS(cvr, cpr, authorization));
            var dig = await gwClient.requestIdCardDigestForSigningAsync(sec, "whatever");


            var csp = cert.GetRSAPrivateKey();
            var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(dig.requestIdCardDigestForSigningResponse.DigestValue);
            var rb = new GW.signIdCardRequestBody
            {
                SignatureValue = csp.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1),
                KeyInfo = new GW.KeyInfo
                {
                    Item = new GW.X509Data { Item = cert.Export(X509ContentType.Cert) }
                }
            };

            var res = await gwClient.signIdCardAsync(sec, rb);
            if (res.signIdCardResponse != GW.signIdCardResponse.ok)
            {
                throw new Exception("Gateway logon error");
            }


            var xAssertion = from element in SerializerUtil.Serialize(sec).Root.Elements()
                             where element.Name.LocalName == "Assertion"
                             select element;


            var idCard = new UserIdCard
            {
                Xassertion = xAssertion.FirstOrDefault()
            };

            return idCard;
        }

        private static GW.Security MakeSecurity(GW.AssertionType assertion)
        {
            return new GW.Security
            {
                id = Guid.NewGuid().ToString("D"),
                Timestamp = new GW.Timestamp { Created = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5) },
                Assertion = assertion
            };
        }

        private static GW.AssertionType MakeAssertionForSTS(string cvr, string cpr = "1802602810", string authorization = "ZXCVB")
        {
            var vnow = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5);

            var ass = new GW.AssertionType
            {
                IssueInstant = vnow,
                id = "IDCard",
                Version = 2.0m,
                Issuer = "WinPLC",
                Conditions = new GW.Conditions
                {
                    NotBefore = vnow,
                    NotOnOrAfter = vnow + TimeSpan.FromHours(8)
                },
                Subject = new GW.Subject
                {
                    NameID = new GW.NameIDType
                    {
                        Format = GW.SubjectIdentifierType.medcomcprnumber,
                        Value = "2203333571"
                    },
                    SubjectConfirmation = new GW.SubjectConfirmation
                    {
                        ConfirmationMethod = GW.ConfirmationMethod.urnoasisnamestcSAML20cmholderofkey,
                        SubjectConfirmationData = new GW.SubjectConfirmationData
                        {
                            Item = new GW.KeyInfo
                            {
                                Item = "OCESSignature"
                            }
                        }
                    }
                },
                AttributeStatement = new[]
                {
                    new GW.AttributeStatement
                    {
                        id = GW.AttributeStatementID.IDCardData,
                        Attribute = new[]
                        {
                            new GW.Attribute {Name = GW.AttributeName.sosiIDCardID, AttributeValue = Guid.NewGuid().ToString("D")},
                            new GW.Attribute {Name = GW.AttributeName.sosiIDCardVersion, AttributeValue = "1.0.1"},
                            new GW.Attribute {Name = GW.AttributeName.sosiIDCardType, AttributeValue = "user"},
                            new GW.Attribute {Name = GW.AttributeName.sosiAuthenticationLevel, AttributeValue = "4"}
                        }
                    },
                    new GW.AttributeStatement
                    {
                        id = GW.AttributeStatementID.UserLog,
                        Attribute = new[]
                        {
                            new GW.Attribute {Name = GW.AttributeName.medcomUserCivilRegistrationNumber, AttributeValue = cpr},
                            new GW.Attribute {Name = GW.AttributeName.medcomUserGivenName, AttributeValue = "Stine"},
                            new GW.Attribute {Name = GW.AttributeName.medcomUserSurName, AttributeValue = "Svendsen"},
                            new GW.Attribute {Name = GW.AttributeName.medcomUserEmailAddress, AttributeValue = "stineSvendsen@example.com"},
                            new GW.Attribute {Name = GW.AttributeName.medcomUserRole, AttributeValue = "læge"},
                            new GW.Attribute {Name = GW.AttributeName.medcomUserAuthorizationCode, AttributeValue = authorization}
                        }
                    },
                    new GW.AttributeStatement
                    {
                        id = GW.AttributeStatementID.SystemLog,
                        Attribute = new[]
                        {
                            new GW.Attribute {Name = GW.AttributeName.medcomITSystemName, AttributeValue = "Sygdom.dk"},
                            new GW.Attribute
                            {
                                Name = GW.AttributeName.medcomCareProviderID,
                                AttributeValue = cvr,
                                NameFormat = GW.SubjectIdentifierType.medcomcvrnumber,
                                NameFormatSpecified = true
                            },
                            new GW.Attribute {Name = GW.AttributeName.medcomCareProviderName, AttributeValue = "Statens Serum Institut"}
                        }
                    }
                }
            };

            return ass;
        }
    }
}