using dk.nsi.seal.Model;
using NUnit.Framework;
using System.Xml.Linq;

namespace SealTest.Model
{
    class EncryptionUtiltTest
    {
        [Test]
        public void testDecrypt()
        {
            var encryptedAssertion = XElement.Load(TestContext.CurrentContext.TestDirectory +
              "/Resources/oiosaml-examples/OIOSAML3Assertion/OIO2BST-encrypted-borger-example.xml");

            var encryptionKey = CredentialVaultTestUtil.GetVoces3CredentialVault();

            // Decrypt bootstrap token
            var decryptedAssertion = new EncryptionUtil().DecryptAssertion(encryptedAssertion, encryptionKey.GetSystemCredentials().PrivateKey);
            Assert.NotNull(decryptedAssertion);

            // Load the decrypted bootstrap token and check CPR.
            var assertion = new OIO2BSTCitizenSAMLAssertion(decryptedAssertion);
            Assert.AreEqual("1802602810", assertion.Cpr);
        }
    }
}
