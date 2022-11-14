using System;
using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public class OcesCertificationAuthority : AbstractOcesCertificationAuthority
    {
        private static readonly string Oces2RootCertificateBase64 =
        "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";

        private static readonly string Oces3RootCertificateBase64 =
        "MIIGfDCCBDCgAwIBAgIUfdkPhrP/1uxz0JMLkAP8WU8ICm8wQQYJKoZIhvcNAQEK" +
        "MDSgDzANBglghkgBZQMEAgMFAKEcMBoGCSqGSIb3DQEBCDANBglghkgBZQMEAgMF" +
        "AKIDAgFAME0xJDAiBgNVBAMMG0RlbiBEYW5za2UgU3RhdCBPQ0VTIHJvZC1DQTEY" +
        "MBYGA1UECgwPRGVuIERhbnNrZSBTdGF0MQswCQYDVQQGEwJESzAeFw0yMTExMDkx" +
        "MjM3MjdaFw00NjExMDMxMjM3MjZaME0xJDAiBgNVBAMMG0RlbiBEYW5za2UgU3Rh" +
        "dCBPQ0VTIHJvZC1DQTEYMBYGA1UECgwPRGVuIERhbnNrZSBTdGF0MQswCQYDVQQG" +
        "EwJESzCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAM1z8k+VAdXUmS9u" +
        "RwPRX2o/vp1F27kjJhzpBtiq87Cncw2jmNjkSXA2KmDL/bfMefLBc9PTRKcufCeB" +
        "6+nb0yJvj7vKhMRkOcbIH4Y0Jn67KBtAWiCN2eINVQgaVX5fmMwJjWNj2aMXG5WY" +
        "HZSSgMSZkiBf3NXeEU8Skg4eqN8qVisR5Rhq9h/MOgD1iPPalgidUG8J7tvxvkHM" +
        "4BA1BbPCtuXH/oTl1pHouTn8SERX2OMQH5oVa1xXo3S33VS/YRydurACIepgALta" +
        "ARKxaQ0Q6InzBJno/Ky8FlM5Pejq5uIlPnN2UjNgYoy5vd+S30eR9ys855fQo/AE" +
        "bJuXK+7PqKs/bykuvkC2obupRfpt6PCCSwidgSqazMl5hoQxzqOVL6KLK+SaS/+f" +
        "KB/t8sQlSXaXOc37PRbE4UZ1useyvmiLDCk4xlkNud8dY9gdg9Bzuad/uXybCJ3x" +
        "PpSyaSotiLQ0qSrU5d7Zm4gaWPamKK0La+pi/LVcslStQKaKvZWIWGAsHP+PSd3q" +
        "cRLhKkSLh7TD6bRrNdS40HC3K2rlkHWpclHHx4DVZx10vR2+DL5XJCMQXqanp/4w" +
        "aJB5Sfw5BJVaYatbY3h+3sWIYVDK/gKj+DjHxGuXP3mTvx4QhJQAbxXsYe9LD0OU" +
        "bTrkIITKVmmFibgnMvIURiVAc8EHAgMBAAGjgeswgegwDwYDVR0TAQH/BAUwAwEB" +
        "/zBrBggrBgEFBQcBAQRfMF0wNwYIKwYBBQUHMAKGK2h0dHA6Ly9jYTEuZ292LmRr" +
        "L29jZXMvcm9vdC9jYWNlcnQvcm9vdC5jZXIwIgYIKwYBBQUHMAGGFmh0dHA6Ly9j" +
        "YTEuZ292LmRrL29jc3AwOQYDVR0fBDIwMDAuoCygKoYoaHR0cDovL2NhMS5nb3Yu" +
        "ZGsvb2Nlcy9yb290L2NybC9yb290LmNybDAdBgNVHQ4EFgQUM2UbcJY08Augh5yD" +
        "3kxPiAytbrYwDgYDVR0PAQH/BAQDAgEGMEEGCSqGSIb3DQEBCjA0oA8wDQYJYIZI" +
        "AWUDBAIDBQChHDAaBgkqhkiG9w0BAQgwDQYJYIZIAWUDBAIDBQCiAwIBQAOCAgEA" +
        "IYh5cL3Ltq5FgWxAzdPyGJZ4YK8c0ud9riLm27LQmyZEXfddilNImIXq36HpDkdc" +
        "RDbI76hJWsUn2fc5F8BY3hSDEkXBmC2YMaDPGmvWDsVxn5wIjdmX3nTvvpkR99BZ" +
        "EcngOYnOlZH/pmL/7OXVjMO/H3MAnzSo91sNlQgHsgKkqVxltVflNvNYkvRAN+Ea" +
        "v5QejsSuZ+kQKHGqMj9AqFc9wC2OCeLsuh/Z3UlgrlVdlxJUZMMP3CEfNpMIVR/t" +
        "VEMzRxkhuYM4NHRDYIu5kjg5w4hJw6K+K8MECoowqgjf9X4dy03HpsLODOVaC+u5" +
        "nM9BknjY0YBi7FDk0NHjPOwFYJoQ3reoZoYWYJ+dWFyQGJoR+FW4nMu6bZbGjsUA" +
        "BSHPqV/cPdC4eVVW9LJZlCHCaGrMdmF2I0wZEqnpfS2kIDgLOWbazG22pBDno3QX" +
        "EHkbTEYWsubkkAsys9/3RIZPlS3ZrFXsPf3VGeXzqTE00WpcJ6P4ffMRyLjXosXG" +
        "3lPMP4eLGml3GfCigEX4RNDZUsA2bv56Od/gzAcKKap3nIgC08MY4zj1OXvrXEEr" +
        "r0CTEI2mqAxN/hYmXVXlsk3Mx8MSm/eYTAzKcAx7wErJvZDrceGUjfe07Z7iXSEk" +
        "wUrknahrmVWcFbzaDuX0p8tbuU5eJs2OTdTkAuiFfIY=";

        public static readonly X509Certificate2 Oces2RootCertificate = new X509Certificate2(Convert.FromBase64String(Oces2RootCertificateBase64));
        public static readonly X509Certificate2 Oces3RootCertificate = new X509Certificate2(Convert.FromBase64String(Oces3RootCertificateBase64));

        public OcesCertificationAuthority(ICertificateStatusChecker certificateStatusChecker) : base(certificateStatusChecker)
        {
        }

        protected override X509Certificate2 GetOCES2RootCertificate()
        {
            return Oces2RootCertificate;
        }

        protected override string GetCertificationAuthorityName()
        {
            return "OCES Production";
        }

        protected override X509Certificate2 GetOCES3RootCertificate()
        {
            return Oces3RootCertificate;
        }
    }
}
