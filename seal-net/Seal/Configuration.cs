using Microsoft.Extensions.Configuration;
using System.IO;

namespace dk.nsi.seal
{
    public static class Configuration
    {
        private static readonly IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, false)
            .Build();

        public static readonly bool CheckTrust = config.GetSection("CheckTrust").Value != bool.FalseString;
        public static readonly bool CheckDate = config.GetSection("CheckDate").Value != bool.FalseString;
        public static readonly bool CheckCrl = config.GetSection("CheckCrl").Value == bool.TrueString;
        public static readonly string SosiDgwsVersion = config.GetSection("sosi:dgws.version").Value.IfEmpty("1.0.1");
        public static readonly string SosiIssuer = config.GetSection("sosi:issuer").Value.IfEmpty("TheSOSILibrary");
        public static readonly string CredentialVaultAlias = config.GetSection("credentialVault:alias").Value.IfEmpty("SOSI:ALIAS_SYSTEM");

        private static string IfEmpty(this string @this, string fallback) =>
            string.IsNullOrEmpty(@this)
                ? fallback
                : @this;
    }
}