﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace dk.nsi.seal.Federation
{
    public class OcesTestCertificationAuthority : AbstractOcesCertificationAuthority
    {

        private static readonly string Oces2TestPpRootCertificateBase64 =
            "MIIGSDCCBDCgAwIBAgIES+pulDANBgkqhkiG9w0BAQsFADBPMQswCQYDVQQGEwJE" +
            "SzESMBAGA1UEChMJVFJVU1QyNDA4MSwwKgYDVQQDEyNUUlVTVDI0MDggU3lzdGVt" +
            "dGVzdCBWSUkgUHJpbWFyeSBDQTAeFw0xMDA1MTIwODMyMTRaFw0zNzAxMTIwOTAy" +
            "MTRaME8xCzAJBgNVBAYTAkRLMRIwEAYDVQQKEwlUUlVTVDI0MDgxLDAqBgNVBAMT" +
            "I1RSVVNUMjQwOCBTeXN0ZW10ZXN0IFZJSSBQcmltYXJ5IENBMIICIjANBgkqhkiG" +
            "9w0BAQEFAAOCAg8AMIICCgKCAgEApuuMpdHu/lXhQ+9TyecthOxrg5hPgxlK1rpj" +
            "syBNDEmOEpmOlK8ghyZ7MnSF3ffsiY+0jA51p+AQfYYuarGgUQVO+VM6E3VUdDpg" +
            "WEksetCYY8L7UrpyDeYx9oywT7E+YXH0vCoug5F9vBPnky7PlfVNaXPfgjh1+66m" +
            "lUD9sV3fiTjDL12GkwOLt35S5BkcqAEYc37HT69N88QugxtaRl8eFBRumj1Mw0LB" +
            "xCwl21GdVY4EjqH1Us7YtRMRJ2nEFTCRWHzm2ryf7BGd80YmtJeL6RoiidwlIgzv" +
            "hoFhv4XdLHwzaQbdb9s141q2s9KDPZCGcgIgeXZdqY1Vz7UBCMiBDG7q2S2ni7wp" +
            "UMBye+iYVkvJD32srGCzpWqG7203cLyZCjq2oWuLkL807/Sk4sYleMA4YFqsazIf" +
            "V+M0OVrJCCCkPysS10n/+ioleM0hnoxQiupujIGPcJMA8anqWueGIaKNZFA/m1IK" +
            "wnn0CTkEm2aGTTEwpzb0+dCATlLyv6Ss3w+D7pqWCXsAVAZmD4pncX+/ASRZQd3o" +
            "SvNQxUQr8EoxEULxSae0CPRyGwQwswGpqmGm8kNPHjIC5ks2mzHZAMyTz3zoU3h/" +
            "QW2T2U2+pZjUeMjYhyrReWRbOIBCizoOaoaNcSnPGUEohGUyLPTbZLpWsm3vjbyk" +
            "7yvPqoUCAwEAAaOCASowggEmMA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0PAQH/BAQD" +
            "AgEGMBEGA1UdIAQKMAgwBgYEVR0gADCBrwYDVR0fBIGnMIGkMDqgOKA2hjRodHRw" +
            "Oi8vY3JsLnN5c3RlbXRlc3Q3LnRydXN0MjQwOC5jb20vc3lzdGVtdGVzdDcuY3Js" +
            "MGagZKBipGAwXjELMAkGA1UEBhMCREsxEjAQBgNVBAoTCVRSVVNUMjQwODEsMCoG" +
            "A1UEAxMjVFJVU1QyNDA4IFN5c3RlbXRlc3QgVklJIFByaW1hcnkgQ0ExDTALBgNV" +
            "BAMTBENSTDEwHwYDVR0jBBgwFoAUI7pMMZDh08zTG7MbWrbIRc3Tg5cwHQYDVR0O" +
            "BBYEFCO6TDGQ4dPM0xuzG1q2yEXN04OXMA0GCSqGSIb3DQEBCwUAA4ICAQCRJ9TM" +
            "7sISJBHQwN8xdey4rxA0qT7NZdKICcIxyIC82HIOGAouKb3oHjIoMgxIUhA3xbU3" +
            "Putr4+Smnc1Ldrw8AofLGlFYG2ypg3cpF9pdHrVdh8QiERozLwfNPDgVeCAnjKPN" +
            "t8mu0FWBS32tiVM5DEOUwDpoDDRF27Ku9qTFH4IYg90wLHfLi+nqc2HwVBUgDt3t" +
            "XU6zK4pzM0CpbrbOXPJOYHMvaw/4Em2r0PZD+QOagcecxPMWI65t2h/USbyO/ah3" +
            "VKnBWDkPsMKjj5jEbBVRnGZdv5rcJb0cHqQ802eztziA4HTbSzBE4oRaVCrhXg/g" +
            "6Jj8/tZlgxRI0JGgAX2dvWQyP4xhbxLNCVXPdvRV0g0ehKvhom1FGjIz975/DMav" +
            "kybh0gzygq4sY9Fykl4oT4rDkDvZLYIxS4u1BrUJJJaDzHCeXmZqOhx8She+Fj9Y" +
            "wVVRGfxT4FL0Qd3WAtaCVyhSQ6SkZgrPvzAmxOUruI6XhEhYGlP5O8WFETiATxuZ" +
            "AJNuKMJtibfRhMNsQ+TVv/ZPr5Swe+3DIQtmt1MIlGlTn4k40z4s6gDGKiFwAYXj" +
            "d/kID32R/hJPE41o9+3nd8aHZhBy2lF0jKAmr5a6Lbhg2O7zjGq7mQ3MceNeebuW" +
            "XD44AxIinryzhqnEWI+BxdlFaia3U7o2+HYdHw==";

        private static readonly string Oces3TestPpRootCerticateBase64 =
            "MIIGsjCCBGagAwIBAgIUVz9X5nUw8aB3ffvGnwkEONM2AlYwQQYJKoZIhvcNAQEK" +
            "MDSgDzANBglghkgBZQMEAgMFAKEcMBoGCSqGSIb3DQEBCDANBglghkgBZQMEAgMF" +
            "AKIDAgFAMGIxJDAiBgNVBAMMG0RlbiBEYW5za2UgU3RhdCBPQ0VTIHJvZC1DQTET" +
            "MBEGA1UECwwKVGVzdCAtIGN0aTEYMBYGA1UECgwPRGVuIERhbnNrZSBTdGF0MQsw" +
            "CQYDVQQGEwJESzAeFw0yMTAxMjgwOTQ5MjVaFw00NjAxMjIwOTQ5MjRaMGIxJDAi" +
            "BgNVBAMMG0RlbiBEYW5za2UgU3RhdCBPQ0VTIHJvZC1DQTETMBEGA1UECwwKVGVz" +
            "dCAtIGN0aTEYMBYGA1UECgwPRGVuIERhbnNrZSBTdGF0MQswCQYDVQQGEwJESzCC" +
            "AiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAL1pnHOstgHrlMGwwnUMP7I6" +
            "uEgZQkzvNcdOF3sjtJh3/i/DwdpAYTEylpf0JQrJl6lJogiukL0wDe08Xvk/NGtc" +
            "0ISWV5nLnD1Vb16RWdInyuz+g7QQepMQljFfsXHixmbjA/ICbmA+/5qDRFHK49zI" +
            "UDRnhDKxgLYGBTFoIvL7mJ3SQJfzoD8Ge0vjUbNvH479k3zmnbOZaht/l4cZcPZH" +
            "0hjdLFqgIaukwNt8Yk14KeMxqNm4KzPpikGnHtZ30xqVNdCGOetSClRB9d8Qx9Hw" +
            "55/vH5/5L9CPAlYH+2TX1l9O+i6kwleE0T7NgVRTTyJitXB7KSPbnDTKH/LVHJBH" +
            "piCQBsqxJguA9k6ujWV2G/Ko2NYkKpMvxc8lQcroMx7LxknEEX/juwkO4aRFaLWQ" +
            "AAJbeo7Av/rPZpYoCN2ri8jVtPq14Aqwx1Y66heYYZPCYMPYD6F22jNcxQE1Y1NI" +
            "GcdIkx8y7j8gP0XgpioRtbz1ej+fw+vGPW1mNQ2Cucg4ZL/o40jwTyigR3qiUa2D" +
            "pDEd1nWYN3X6N1CBz1EYCGlQPNcuQ2IponQlyRCmBQ+e9N9ouPajdcq7QjAZQE78" +
            "mhSwnF996NRnjdg5sMBqRvVR7DpNRb51LitiLJRiC+gOQy94bI4Qu5ewG5ZHHNf6" +
            "PTpZ3g5GV70DsBmnAtrhAgMBAAGjgfcwgfQwDwYDVR0TAQH/BAUwAwEB/zBzBggr" +
            "BgEFBQcBAQRnMGUwOwYIKwYBBQUHMAKGL2h0dHA6Ly9jYTEuY3RpLWdvdi5kay9v" +
            "Y2VzL3Jvb3QvY2FjZXJ0L3Jvb3QuY2VyMCYGCCsGAQUFBzABhhpodHRwOi8vY2Ex" +
            "LmN0aS1nb3YuZGsvb2NzcDA9BgNVHR8ENjA0MDKgMKAuhixodHRwOi8vY2ExLmN0" +
            "aS1nb3YuZGsvb2Nlcy9yb290L2NybC9yb290LmNybDAdBgNVHQ4EFgQUOdze3tCQ" +
            "Jkeg4MZuSX8m8qkv9hswDgYDVR0PAQH/BAQDAgEGMEEGCSqGSIb3DQEBCjA0oA8w" +
            "DQYJYIZIAWUDBAIDBQChHDAaBgkqhkiG9w0BAQgwDQYJYIZIAWUDBAIDBQCiAwIB" +
            "QAOCAgEABzNBub3P/ND9hpsevGZSm0mz0OhvkRUGU1JLKpCmaXMx4lb86gxzpj4C" +
            "6wjxWfofNCsRWlLEN52aJslFjzFm7LYvi+S9LTp+yxas7JSFXXkViaDfi2y+FiIZ" +
            "IJhkQWOV1gzkGZwjTEqhpzXnYJYTyLk/MZ1E1o0EcuxDFk2izpUFDR2TdYrCy8xQ" +
            "YRTOVdB/oW7nljdQYEzaDrrnQwanAnNkqU5Lse5DQ8P5aATF+BBL7QFQwqnMiFeY" +
            "2THu948ggl7oTuaPrFysWv9JrxWRQD9NX4pC1mgUkgJnejGOihaRKjVDcJMxCe4Y" +
            "B5tpX+JhaNO+gW4YlwxSgFAb7UG+nF8qtGCRoAZQaPzWVokYyYHSW/qLMCPySiWA" +
            "9GlMEGDuiZF/xkO96vtNA2MCJnPQCW1OJw+v5IQWWFuYXCYMNzepfiGUNB+ExYP2" +
            "wuRkMOm2EBCWt8vay2/aUEQSrWAd2BuT/nUWt7vkT7cQRtiu89axJ3SZ8P+QwDr+" +
            "zoEF82tKZJ2hXuVjVaiEwxnDmv+UuQMkl52LY3XRfa6F7zO9CoTbPyCazkp+6YWg" +
            "OdttM00ic5kscFT5kOH/0Pb6pILDWDnJ5L8XqVxb3zRXO/DM/IAwF2uV1PbLq7ly" +
            "LTiVhleQahXJ2SeRnon8KdjMiCdjlpOxHZZgEENPf08RSOQqEiI=";

        public static readonly X509Certificate2 Oces2TestRootCertificate = new X509Certificate2(Convert.FromBase64String(Oces2TestPpRootCertificateBase64));

        public static readonly X509Certificate2 Oces3TestRootCertificate = new X509Certificate2(Convert.FromBase64String(Oces3TestPpRootCerticateBase64));

        public OcesTestCertificationAuthority(ICertificateStatusChecker certificateStatusChecker) : base(certificateStatusChecker)
        {
        }

        protected override X509Certificate2 GetOCES2RootCertificate()
        {
            return Oces2TestRootCertificate;
        }

        protected override string GetCertificationAuthorityName()
        {
            return "OCES Test";
        }

        protected override X509Certificate2 GetOCES3RootCertificate()
        {
            return Oces3TestRootCertificate;
        }
    }
}
