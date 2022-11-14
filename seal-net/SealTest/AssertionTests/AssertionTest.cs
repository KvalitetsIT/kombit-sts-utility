using System;
using System.ServiceModel;
using System.Threading.Tasks;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using NUnit.Framework;
using SealTest.Certificate;
using SealTest.Model;
using SealTest.NSTWsProvider;
using Assert = NUnit.Framework.Assert;
using static dk.nsi.seal.MessageHeaders.XmlMessageHeader;
using static dk.nsi.seal.MessageHeaders.IdCardMessageHeader;

namespace SealTest.AssertionTests
{
    using proxy = dk.nsi.fmk;

    [TestFixture(typeof(CertificateSuiteOces2), "1802602810", "ZXCVB")]
    [TestFixture(typeof(CertificateSuiteOces3), "0306894781", "KT2Z4")]
    public class AssertionTest<CERTIFICATE_SUITE> : AbstractTest where CERTIFICATE_SUITE : ICertificateSuite, new()
    {
        private CERTIFICATE_SUITE certificateSuite;
        private readonly string mocesCpr;
        private readonly string authorization;

        public AssertionTest(string mocesCpr, string authorization)
        {
            certificateSuite = new CERTIFICATE_SUITE();
            this.mocesCpr = mocesCpr;
            this.authorization = authorization;
        }

        private static Task CallNts(IdCard idc, int securityLevel)
        {
            var client = new NtsWSProviderClient(new BasicHttpsBinding(), new EndpointAddress("https://test1-cnsp.ekstern-test.nspop.dk:8443/nts/service"));
            client.Endpoint.EndpointBehaviors.Add(new SealEndpointBehavior());

            using (new OperationContextScope(client.InnerChannel))
            {
                // Adding seal-security and dgws-header soap header
                OperationContext.Current.OutgoingMessageHeaders.Add(IdCardHeader(idc));
                OperationContext.Current.OutgoingMessageHeaders.Add(XmlHeader(MakeDgwsHeader(securityLevel)));

                // Throws Exception if not succesful.
                return client.invokeAsync("test");
            }
        }

        [Test]
        public Task TestStsAndNtsAssertionAsTypeMoces()
        {
            var ass = AssertionMaker.MakeAssertionForSTS(certificateSuite.MocesCprValid, 4, mocesCpr, authorization);
            var card = new SystemIdCard { Xassertion = SerializerUtil.Serialize(ass).Root };

            var sCard = SealUtilities.SignIn(card, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(sCard, 4);
        }

        [Test]
        public Task TestStsAndNtsAssertionAsTypeMoces_new()
        {
            var factory = CreateSOSIFactory(certificateSuite.MocesCprValid.Certificate);
            var uid = CreateIdCardForSTS(certificateSuite.MocesCprValid, mocesCpr, authorization);
            uid.Sign<Assertion>(factory.SignatureProvider);

            var idc = SealUtilities.SignIn(uid, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(idc, 4);
        }

        [Test]
        public Task TestStsAndNstAssertionAsTypeVoces()
        {
            var ass = AssertionMaker.MakeAssertionForSTS(certificateSuite.VocesValid, 3);
            var card = new SystemIdCard { Xassertion = SerializerUtil.Serialize(ass).Root };
            var sCard = SealUtilities.SignIn(card, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(sCard, 3);
        }

        [Test]
        public Task TestStsAndNtsAssertionAsTypeVoces_new()
        {
            var factory = CreateSOSIFactory(certificateSuite.VocesValid.Certificate);
            var uid = CreateVocesSystemIdCard(certificateSuite.VocesValid);
            uid.Sign<Assertion>(factory.SignatureProvider);

            var idc = SealUtilities.SignIn(uid, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);
            return CallNts(idc, 3);
        }

        [Test]
        public Task TestStsAndNtsAssertionAsXmlMoces()
        {
            var ast = AssertionMaker.MakeAssertionForSTS(certificateSuite.MocesCprValid, 4, mocesCpr, authorization);
            var card = new SystemIdCard { Xassertion = SerializerUtil.Serialize(ast).Root };

            var sc = SealUtilities.SignIn(card, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(sc, 4);
        }

        [Test]
        public Task TestStsAndNtsAssertionAsXmlMoces_new()
        {
            var factory = CreateSOSIFactory(certificateSuite.MocesCprValid.Certificate);
            var uid = CreateIdCardForSTS(certificateSuite.MocesCprValid, mocesCpr, authorization);
            uid.Sign<Assertion>(factory.SignatureProvider);

            var idc = SealUtilities.SignIn(uid, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(idc, 4);
        }


        [Test]
        public Task TestStsAndNtsAssertionAsXmlVoces()
        {
            var ast = AssertionMaker.MakeAssertionForSTS(certificateSuite.VocesValid, 3);
            var card = new SystemIdCard { Xassertion = SerializerUtil.Serialize(ast).Root };
            var sCard = SealUtilities.SignIn(card, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(sCard, 3);
        }

        [Test]
        public Task TestStsAndNtsAssertionAsXmlVoces_new()
        {
            //Seal kort oprettes 
            //FMK kaldes
            //Assertion overføres via SealCard som XML
            var factory = CreateSOSIFactory(certificateSuite.VocesValid.Certificate);
            var uid = CreateVocesSystemIdCard(certificateSuite.VocesValid);
            uid.Sign<Assertion>(factory.SignatureProvider);

            var idc = SealUtilities.SignIn(uid, "http://www.ribeamt.dk/EPJ", TestConstants.SecurityTokenService);

            return CallNts(idc, 3);
        }

        [Test]
        public void TestAssertionSignMoces()
        {
            var ass = SealUtilities.SignAssertion(AssertionMaker.MakeAssertion(), certificateSuite.MocesCprValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(ass));

            var sec = MakeSecurity(AssertionMaker.MakeAssertion());
            sec = SealUtilities.SignAssertion(sec, certificateSuite.MocesCprValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(sec));
        }

        [Test]
        public void TestAssertionSignMoces_new()
        {
            var factory = CreateSOSIFactory(certificateSuite.MocesCprValid.Certificate);
            var uid = CreateMocesUserIdCard();

            var ass = uid.Sign<dk.nsi.fmk.Assertion>(factory.SignatureProvider);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(ass));

            var uid2 = CreateMocesUserIdCard();

            var sec = MakeSecurity(uid2.GetAssertion<dk.nsi.fmk.Assertion>());
            sec = SealUtilities.SignAssertion(sec, certificateSuite.MocesCprValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(sec));
        }

        [Test]
        public void TestAssertionSignVoces()
        {
            var ass = SealUtilities.SignAssertion(AssertionMaker.MakeAssertion(), certificateSuite.VocesValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(ass));

            var sec = MakeSecurity(AssertionMaker.MakeAssertion());
            sec = SealUtilities.SignAssertion(sec, certificateSuite.VocesValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(sec));
        }

        [Test]
        public void TestAssertionSignVoces_new()
        {
            var factory = CreateSOSIFactory(certificateSuite.VocesValid.Certificate);
            var uid = CreateMocesUserIdCard();

            var ass = uid.Sign<dk.nsi.fmk.Assertion>(factory.SignatureProvider);

            Assert.IsTrue(SealUtilities.CheckAssertionSignature(ass));

            var uid2 = CreateMocesUserIdCard();

            var sec = MakeSecurity(uid2.GetAssertion<dk.nsi.fmk.Assertion>());
            sec = SealUtilities.SignAssertion(sec, certificateSuite.VocesValid.Certificate);
            Assert.IsTrue(SealUtilities.CheckAssertionSignature(sec));
        }

        private static proxy.Security MakeSecurity(proxy.Assertion assertion)
        {
            return new proxy.Security
            {
                id = Guid.NewGuid().ToString("D"),
                Timestamp = new proxy.Timestamp {Created = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5)},
                Assertion = assertion
            };
        }

        private static proxy.Header MakeDgwsHeader(int securityLevel) => new()
        {
            SecurityLevel = securityLevel,
            SecurityLevelSpecified = true,
            TimeOut = proxy.TimeOut.Item1440,
            TimeOutSpecified = true,
            Linking = new proxy.Linking
            {
                FlowID = Guid.NewGuid().ToString("D"),
                MessageID = Guid.NewGuid().ToString("D")
            },
            FlowStatus = proxy.FlowStatus.flow_running,
            FlowStatusSpecified = true,
            Priority = proxy.Priority.RUTINE,
            RequireNonRepudiationReceipt = proxy.RequireNonRepudiationReceipt.yes
        };
    }
}