using dk.nsi.seal.Factories;
using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Vault;
using System;

namespace SealTest.AssertionTests.AssertionBuilders
{
    public class OIOBSTAssertionBuilderHelper
    {
        private static readonly string UUID_WITH_0_NATIONAL_ROLES_0_AUTHORIZATIONS = "71cf2619-531a-4543-9171-5212a1956e53";
        private static readonly string UUID_WITH_0_NATIONAL_ROLES_3_AUTHORIZATIONS = "dc7f4a98-d4f9-4d77-84f9-ae83c4e0bf61";
        private static readonly string UUID_WITH_3_NATIONAL_ROLES_0_AUTHORIZATIONS = "e9c66150-aef3-4fe1-842b-08311b27106b";
        private static readonly string CPR_WITH_0_AUTHORIZATIONS = "1212714321";

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> CreateOIOH2RequestBuilderFromAssertionBuilder(ICredentialVault signingVault)
        {
            var requestBuilder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionToIDCardRequestDOMBuilder();
            requestBuilder.Audience = "https://fmk";
            requestBuilder.ItSystemName = "Korsbæk Kommunes IT systemer";
            requestBuilder.SigningVault = signingVault;
            requestBuilder.SubjectNameId = "Mads_Skjern";

            return requestBuilder;
        }

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion> CreateOIOH3RequestBuilderFromAssertionBuilder(ICredentialVault signingVault)
        {
            OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion> requestBuilder = OIOSAMLFactory.CreateOIOH3BSTSAMLAssertionToIDCardRequestDOMBuilder();
            requestBuilder.Audience = "https://fmk";
            requestBuilder.ItSystemName = "Korsbæk Kommunes IT systemer";
            requestBuilder.SigningVault = signingVault;
            requestBuilder.SubjectNameId = "Mads_Skjern";

            return requestBuilder;
        }

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion> CreateOIO3RequestBuilderFromAssertionBuilder(ICredentialVault signingVault)
        {
            OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion> requestBuilder = OIOSAMLFactory.CreateOIO3BSTSAMLAssertionToIDCardRequestDOMBuilder();
            requestBuilder.Audience = "https://fmk";
            requestBuilder.ItSystemName = "Korsbæk Kommunes IT systemer";
            requestBuilder.SigningVault = signingVault;
            requestBuilder.SubjectNameId = "Mads_Skjern";

            return requestBuilder;
        }

        public static OIOH3BSTSAMLAssertionBuilder CreateOIOH3BSTSAMLAssertionBuilder(ICredentialVault holderOfKeyVault, ICredentialVault signingVault)
        {
            OIOH3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH3BSTSAMLAssertionBuilder();
            builder.Issuer = "https://oioh3bst-issuer.dk";
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            builder.Audience = "https://sts.sosi.dk/";
            builder.NotOnOrAfter = DateTime.Now.AddHours(2);
            builder.SigningVault = signingVault;
            builder.SetAssuranceLevel("NSIS", "High");
            builder.Uuid = UUID_WITH_0_NATIONAL_ROLES_3_AUTHORIZATIONS;
            builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";
            return builder;
        }

        public static OIO3BSTSAMLAssertionBuilder CreateOIO3BSTSAMLAssertionBuilder(ICredentialVault holderOfKeyVault, ICredentialVault signingVault)
        {
            OIO3BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIO3BSTSAMLAssertionBuilder();
            builder.Issuer = "https://oio3bst-issuer.dk";
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            builder.Audience = "https://sts.sosi.dk/";
            builder.NotOnOrAfter = DateTime.Now.AddHours(2);
            builder.SigningVault = signingVault;
            builder.SetAssuranceLevel("NIST", "4");
            builder.Cpr = "1010734321";
            builder.Uuid = UUID_WITH_3_NATIONAL_ROLES_0_AUTHORIZATIONS;
            builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";
            return builder;
        }

        public static OIOH2BSTSAMLAssertionBuilder CreateOIOH2BSTSAMLAssertionBuilder(ICredentialVault holderOfKeyVault, ICredentialVault signingVault)
        {
            OIOH2BSTSAMLAssertionBuilder builder = OIOSAMLFactory.CreateOIOH2BSTSAMLAssertionBuilder();
            builder.Issuer = "https://oioh2bst-issuer.dk";
            builder.NameId = "KorsbaekKommune\\MSK";
            builder.HolderOfKeyCertificate = holderOfKeyVault.GetSystemCredentials();
            builder.Audience = "https://sts.sosi.dk/";
            builder.NotOnOrAfter = DateTime.Now.AddHours(2);
            builder.SigningVault = signingVault;
            builder.SetAssuranceLevel("NIST", "4");
            builder.Uuid = UUID_WITH_0_NATIONAL_ROLES_0_AUTHORIZATIONS;
            builder.Cvr = "20301823";
            builder.OrganizationName = "Korsbæk Kommune";
            builder.Cpr = CPR_WITH_0_AUTHORIZATIONS;
            return builder;
        }
    }
}
