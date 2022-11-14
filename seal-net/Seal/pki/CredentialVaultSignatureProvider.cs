using System;
using System.Xml.Linq;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Vault;

namespace dk.nsi.seal.pki
{
    public class CredentialVaultSignatureProvider : ISignatureProvider
    {
        public ICredentialVault Vault { get; }

        public CredentialVaultSignatureProvider(ICredentialVault vault) => 
            Vault = vault ?? throw new ArgumentException("CredentialVault cannot be null");

        public XElement Sign(Assertion ass)
        {
            ass = SealUtilities.SignAssertion(ass, Vault.GetSystemCredentials());
            return SerializerUtil.Serialize(ass).Root;
        }
    }
}
