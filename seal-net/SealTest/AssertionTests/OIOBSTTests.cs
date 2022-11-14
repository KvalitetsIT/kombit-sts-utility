using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Factories;
using dk.nsi.seal.Federation;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using NUnit.Framework;
using SealTest.AssertionTests.AssertionBuilders;
using SealTest.NSTWsProvider;
using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static dk.nsi.seal.MessageHeaders.XmlMessageHeader;
using static dk.nsi.seal.MessageHeaders.IdCardMessageHeader;

namespace SealTest.AssertionTests
{
    class OIOBSTTests
    {
        private static readonly Federation federation = new SosiTestFederation(new CrlCertificateStatusChecker());

        private static readonly ICredentialVault holderOfKeyVault = CredentialVaultTestUtil.GetFoces3CredentialVault();
        private static readonly ICredentialVault issuerSigningVault = CredentialVaultTestUtil.GetVoces3CredentialVault();

        private OIOH3BSTSAMLAssertionBuilder assertionBuilderOIOH3Bst;
        private OIO3BSTSAMLAssertionBuilder assertionBuilderOIO3Bst;
        private OIOH2BSTSAMLAssertionBuilder assertionBuilderOIOH2Bst;

        private OIOH3BSTSAMLAssertion assertionOIOH3Bst;

        private OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion> oioh3RequestBuilder;
        private OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion> oio3RequestBuilder;
        private OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> oioh2RequestBuilder;

        /// <summary>
        /// Exchanges OIOH3BST boot strap token to SOSI ID card. OIOH3BST is a boot strap token issued
        /// by a local IDP. They are based on OIOSAML 3.x. The boot strap token is not encrypted.
        /// 
        /// Output: Valid OIOBSTSAMLAssertionToIDCardResponse
        /// </summary>
        [Test]
        public Task TestOIOH3BST()
        {
            SetupOIOH3BstTest();

            oioh3RequestBuilder.UserAuthorizationCode = "KT2Z4"; // Findes i AuthorizationServiceStub for cpr 1802602810
            assertionBuilderOIOH3Bst.ValidateBeforeBuild();
            oioh3RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIOH3Bst.Build());

            var response = SendRequest(oioh3RequestBuilder.Build());

            Assert.False(response.IsFault, response.FaultString);
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(federation);
            Assert.NotNull(response.Created);
            Assert.NotNull(response.Expires);
            UserIdCard idCard = (UserIdCard)response.IdCard;
            Assert.AreEqual("Mads_Skjern", idCard.AlternativeIdentifier, "Uventet Alternativ Identifier/Subject Name ID");
            Assert.AreEqual("KT2Z4", idCard.UserInfo.AuthorizationCode);


            var client = new NtsWSProviderClient(new BasicHttpsBinding(), new EndpointAddress("https://test1-cnsp.ekstern-test.nspop.dk:8443/nts/service"));
            client.Endpoint.EndpointBehaviors.Add(new SealEndpointBehavior());

            using (new OperationContextScope(client.InnerChannel))
            {
                // Adding seal-security and dgws-header soap header
                OperationContext.Current.OutgoingMessageHeaders.Add(IdCardHeader(idCard));
                OperationContext.Current.OutgoingMessageHeaders.Add(XmlHeader(MakeDgwsHeaderForNts()));
                // Throws Exception if not succesful.
                return client.invokeAsync("test");
            }
        }

        private static Header MakeDgwsHeaderForNts() => new()
        {
            SecurityLevel = 4,
            SecurityLevelSpecified = true,
            Linking = new Linking { MessageID = Guid.NewGuid().ToString("D") }
        };

        /// <summary>
        /// Exchanges OIOH3BST boot strap token to SOSI ID card. OIOH3BST is a boot strap token issued
        /// by a local IDP. They are based on OIOSAML 3.x. The boot strap token is not encrypted. 
        /// The OIOH3BST Assertion is loaded from a string. 
        /// 
        /// Output: Valid OIOBSTSAMLAssertionToIDCardResponse
        /// </summary>
        [Test]
        public void TestOIOH3BSTFromString()
        {
            SetupOIOH3BstTestFromString();

            oioh3RequestBuilder.UserAuthorizationCode = "KT2Z4"; // Findes i AuthorizationServiceStub for cpr 1802602810
            oioh3RequestBuilder.SetOIOSAMLAssertion(assertionOIOH3Bst);

            var response = SendRequest(oioh3RequestBuilder.Build());

            Assert.False(response.IsFault, response.FaultString);
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(federation);
            Assert.NotNull(response.Created);
            Assert.NotNull(response.Expires);
            UserIdCard idCard = (UserIdCard)response.IdCard;
            Assert.AreEqual("Mads_Skjern", idCard.AlternativeIdentifier, "Uventet Alternativ Identifier/Subject Name ID");
            Assert.AreEqual("KT2Z4", idCard.UserInfo.AuthorizationCode);
        }

        /// <summary>
        /// Tries to exchanges OIOH3BST boot strap token to SOSI ID card. OIOH3BST is a boot strap token issued
        /// by a local IDP. They are based on OIOSAML 3.x. The boot strap token is not encrypted.
        /// 
        /// Output: Error due to validation error.
        /// 
        /// </summary>
        [Test]
        public void TestOIOH3BSTInvalidAuthorization()
        {
            SetupOIOH3BstTest();

            oioh3RequestBuilder.UserAuthorizationCode = "005Nz"; // Findes IKKE i AuthorizationServiceStub for cpr 1802602810
            assertionBuilderOIOH3Bst.ValidateBeforeBuild();
            oioh3RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIOH3Bst.Build());

            var response = SendRequest(oioh3RequestBuilder.Build());

            Assert.True(response.IsFault);
            Assert.AreEqual("dk:sosi:sts:autorisation", response.FaultActor, "Unexpected fault actor: " + response.FaultActor);
            Assert.True(response.FaultString.Contains("Authentication failed: authorization not found"), response.FaultString);
            Assert.AreEqual("wst:FailedAuthentication", response.FaultCode);

        }

        /// <summary>
        /// Exchanges OIO3BST boot strap token to SOSI ID card. OIO3BST is a boot strap token issued
        /// NemLog-in3 STS. They are based on OIOSAML 3.x. The boot strap token is encrypted.
        /// 
        /// Output: Valid OIOBSTSAMLAssertionToIDCardResponse
        /// </summary>
        [Test]
        public void TestOIO3BST()
        {
            SetupOIO3BstTest();

            AddNationalRoleToRequest(oio3RequestBuilder, "41002", "SundAssistR2");
            assertionBuilderOIO3Bst.ValidateBeforeBuild();
            oio3RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIO3Bst.Build());

            OIOBSTSAMLAssertionToIDCardResponse response = SendRequest(oio3RequestBuilder.Build());
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(federation);
            Assert.NotNull(response.Created);
            Assert.NotNull(response.Expires);

            UserIdCard idCard = (UserIdCard)response.IdCard;
            Assert.True(idCard is UserIdCard, "Unexpected id card");
            Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:41002:value:SundAssistR2", idCard.UserInfo.Role, "Unexpected user role");
        }

        /// <summary>
        /// Exchanges OIO3BST boot strap token to SOSI ID card. OIO3BST is a boot strap token issued
        /// NemLog-in3 STS. They are based on OIOSAML 3.x. The boot strap token is encrypted.
        /// 
        /// Output: Error due to validation error.
        /// </summary>
        [Test]
        public void TestOIO3BSTInvalidNationalRole()
        {
            SetupOIO3BstTest();

            AddNationalRoleToRequest(oio3RequestBuilder, "41002", "WrongRole");
            assertionBuilderOIO3Bst.ValidateBeforeBuild();
            oio3RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIO3Bst.Build());

            OIOBSTSAMLAssertionToIDCardResponse response = SendRequest(oio3RequestBuilder.Build());

            Assert.True(response.IsFault);
            Assert.AreEqual("dk:sosi:sts:autorisation", response.FaultActor, "Unexpected fault actor: " + response.FaultActor);
            Assert.AreEqual("Authentication failed: invalid role supplied (urn:dk:healthcare:national-federation-role:code:41002:value:WrongRole)", response.FaultString, response.FaultString);
            Assert.AreEqual("wst:FailedAuthentication", response.FaultCode);
        }

        /// <summary>
        /// Exchanges OIOH2BST boot strap token to SOSI ID card. OIO2BST is a boot strap token issued
        /// SEB. They are based on OIOSAML 2.x. The boot strap token is encrypted.
        /// 
        /// Output: Valid OIOBSTSAMLAssertionToIDCardResponse
        /// </summary>
        [Test]
        public void TestOIO2HBST()
        {
            SetupOIOH2BstTest();

            BasicPrivilegesDOMBuilder privilegesBuilder = new();
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "00001", "role1");
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "00002", "role2");
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "99991", "TestRolle1");
            AddNationalRoleToRequest(oioh2RequestBuilder, "99991", "TestRolle1");

            assertionBuilderOIOH2Bst.ValidateBeforeBuild();
            oioh2RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIOH2Bst.Build());

            OIOBSTSAMLAssertionToIDCardResponse response = SendRequest(oioh2RequestBuilder.Build());
            response.ValidateSignature();
            response.ValidateSignatureAndTrust(federation);
            Assert.NotNull(response.Created);
            Assert.NotNull(response.Expires);

            var idCard = response.IdCard; ;
            Assert.True(idCard is UserIdCard, "Unexpected id card");
            Assert.AreEqual("urn:dk:healthcare:national-federation-role:code:99991:value:TestRolle1", ((UserIdCard)idCard).UserInfo.Role, "Unexpected user role");
        }

        /// <summary>
        /// Exchanges OIOH2BST boot strap token to SOSI ID card. OIO2BST is a boot strap token issued
        /// SEB. They are based on OIOSAML 2.x. The boot strap token is encrypted.
        /// 
        /// Output: Error due to validation error.
        /// </summary>
        [Test]
        public void TestOIO2HBSTInvalidNationalRole()
        {
            SetupOIOH2BstTest();

            BasicPrivilegesDOMBuilder privilegesBuilder = new();
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "00001", "role1");
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "00002", "role2");
            AddNationalRoleToBst(assertionBuilderOIOH2Bst, privilegesBuilder, "00003", "role3");
            string role = AddNationalRoleToRequest(oioh2RequestBuilder, "99991", "TestRolle1");

            assertionBuilderOIOH2Bst.ValidateBeforeBuild();
            oioh2RequestBuilder.SetOIOSAMLAssertion(assertionBuilderOIOH2Bst.Build());

            OIOBSTSAMLAssertionToIDCardResponse response = SendRequest(oioh2RequestBuilder.Build());

            Assert.True(response.IsFault, "Not a fault");
            Assert.AreEqual("dk:sosi:sts:autorisation", response.FaultActor, "Unexpected fault actor: " + response.FaultActor);
            Assert.True(response.FaultString.Contains("Authentication failed: user does not have the requested role (" + role + ")"), "Unexpected fault string: " + response.FaultString);
            Assert.AreEqual("wst:FailedAuthentication", response.FaultCode);
        }

        private void AddNationalRoleToBst(OIOH2BSTSAMLAssertionBuilder assertionBuilder, BasicPrivilegesDOMBuilder privilegesBuilder, String code, String value)
        {
            privilegesBuilder.AddPrivilege("urn:dk:gov:saml:cvrNumberIdentifier:20301823", $"urn:dk:healthcare:national-federation-role:code:{code}:value:{value}");
            assertionBuilder.Privileges = privilegesBuilder;
        }

        private void AddNationalRoleToRequest(OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion> requestBuilder, String code, String value)
        {
            requestBuilder.UserAuthorizationCode = null;
            requestBuilder.UserRole = $"urn:dk:healthcare:national-federation-role:code:{code}:value:{value}";
        }

        private string AddNationalRoleToRequest(OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> requestBuilder, String code, String value)
        {
            string role = $"urn:dk:healthcare:national-federation-role:code:{code}:value:{value}";
            requestBuilder.UserAuthorizationCode = null;
            requestBuilder.UserRole = role;

            return role;
        }

        public static OIOBSTSAMLAssertionToIDCardResponse SendRequest(XDocument xDocument)
        {
            var response = WebPost(xDocument.Root, TestConstants.SecurityTokenServiceBst2Sosi);

            var modelBuilder = OIOSAMLFactory.CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder();

            return modelBuilder.Build(new XDocument(response));
        }

        void SetupOIOH2BstTest()
        {
            assertionBuilderOIOH2Bst = OIOBSTAssertionBuilderHelper.CreateOIOH2BSTSAMLAssertionBuilder(holderOfKeyVault, issuerSigningVault);
            oioh2RequestBuilder = OIOBSTAssertionBuilderHelper.CreateOIOH2RequestBuilderFromAssertionBuilder(holderOfKeyVault);
            oioh2RequestBuilder.EncryptionKey = issuerSigningVault.GetSystemCredentials().PublicKey;
        }

        void SetupOIOH3BstTest()
        {
            assertionBuilderOIOH3Bst = OIOBSTAssertionBuilderHelper.CreateOIOH3BSTSAMLAssertionBuilder(holderOfKeyVault, issuerSigningVault);
            oioh3RequestBuilder = OIOBSTAssertionBuilderHelper.CreateOIOH3RequestBuilderFromAssertionBuilder(holderOfKeyVault);
        }

        void SetupOIOH3BstTestFromString()
        {
            // Manually generate BST
            assertionBuilderOIOH3Bst = OIOBSTAssertionBuilderHelper.CreateOIOH3BSTSAMLAssertionBuilder(holderOfKeyVault, issuerSigningVault);
            var bst = assertionBuilderOIOH3Bst.Build();

            // Convert it to a string
            var reader = bst.XAssertion.CreateReader();
            reader.MoveToContent();

            // This is the BST as a string. 
            var bstAsString = bst.XAssertion.ToString(SaveOptions.DisableFormatting);

            // Load the string in OIOH3BSTAssertion
            assertionOIOH3Bst = new OIOH3BSTSAMLAssertion(XElement.Parse(bstAsString));

            // Create a request builder.
            oioh3RequestBuilder = OIOBSTAssertionBuilderHelper.CreateOIOH3RequestBuilderFromAssertionBuilder(holderOfKeyVault);
        }

        private void SetupOIO3BstTest()
        {
            assertionBuilderOIO3Bst = OIOBSTAssertionBuilderHelper.CreateOIO3BSTSAMLAssertionBuilder(holderOfKeyVault, issuerSigningVault);
            oio3RequestBuilder = OIOBSTAssertionBuilderHelper.CreateOIO3RequestBuilderFromAssertionBuilder(holderOfKeyVault);
            oio3RequestBuilder.EncryptionKey = issuerSigningVault.GetSystemCredentials().PublicKey;
        }

        public static XElement WebPost(XElement request, string url)
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
    }
}
