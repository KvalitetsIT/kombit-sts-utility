using System;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;
using NUnit.Framework;

namespace SealTest
{
	public class CredentialVaultTestUtil
	{
		public const string XML_DOCUMENT =
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
			"<person xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns=\"http://www.sosi.dk/\" >\n" +
			"  <fornavn id=\"elmtosign\"><value>Hans</value></fornavn>\n" + "  <efternavn><value>Hansen</value></efternavn>\n"
			+ "</person>\n";

		public const string VOCES_EXPIRED_PKCS12_PWD = "!234Qwer";

		public const string VOCES_EXPIRED_PKCS12 =
			"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";



        //private const Properties properties = SignatureUtil.setupCryptoProviderForJVM();

        /**
		 * DGWS Envelope with no signatures in it
		 */

        public const string XML_DOCUMENT_4 =
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + "<soap:Envelope \n" +
			"\txmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
			"\txmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\"\n" +
			" \txmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"\n"
			+ " \txmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"\n" +
			" \txmlns:medcom=\"http://www.medcom.dk/dgws/2006/04/dgws-1.0.xsd\"\n" +
			" \txmlns:sosi=\"http://www.sosi.dk/sosi/2006/04/sosi-1.0.xsd\"\n" +
			" \txmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"\n"
			+ " \txmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"\n" +
			" \tid=\"Envelope\">\n" + "\t<soap:Header>\n" + "\t\t<wsse:Security>\n" + "\t\t\t<wsu:Timestamp>\n" +
			"\t\t\t\t<wsu:Created>2005-08-24T10:03:46</wsu:Created>\n" + "\t\t\t</wsu:Timestamp>\n" + "\t\t\t<saml:Assertion \n"
			+ "\t\t\t\tid=\"IDCard\"\n" + "\t\t\t\tIssueInstant=\"2006-01-05T07:53:00\" \n" + "\t\t\t\tVersion=\"2.0\">\n" +
			"\t\t\t\t<saml:Issuer>some.system.name</saml:Issuer>\n" + "\t\t\t\t<saml:Subject>\n" +
			"\t\t\t\t\t<saml:NameID Format=\"medcom:cprnumber\">\n" + "\t\t\t\t\t\t1903701234\n" + "\t\t\t\t\t</saml:NameID>\n"
			+ "\t\t\t\t\t<saml:SubjectConfirmation>\n" +
			"\t\t\t\t\t\t<saml:ConfirmationMethod>urn:oasis:names:tc:SAML:2.0:cm:holder-of-key</saml:ConfirmationMethod>\n" +
			"\t\t\t\t\t\t<saml:SubjectConfirmationData>\n" + "\t\t\t\t\t\t\t<ds:KeyInfo>\n" +
			"\t\t\t\t\t\t\t\t<ds:KeyName>OCESSignature</ds:KeyName>\n" + "\t\t\t\t\t\t\t</ds:KeyInfo>\n"
			+ "\t\t\t\t\t\t</saml:SubjectConfirmationData>\n" + "\t\t\t\t\t</saml:SubjectConfirmation>\n" +
			"\t\t\t\t</saml:Subject>\n" + "\t\t\t\t<saml:Conditions \n" + "\t\t\t\t\tNotBefore=\"2006-01-05T07:53:00.00\"\n" +
			"\t\t\t\t\tNotOnOrAfter=\"2006-01-06T07:53:00.000\"/>\n" + "\t\t\t\t<saml:AttributeStatement id=\"IDCardData\">\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"sosi:IDCardID\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>1234</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"sosi:IDCardVersion\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>1.0</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"sosi:IDCardType\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>user</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"sosi:AuthenticationLevel\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>4</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"sosi:OCESCertHash\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>ALiLaerBquie1/t6ykRKqLZe13Y=</saml:AttributeValue>\n" +
			"\t\t\t\t\t</saml:Attribute>\n" + "\t\t\t\t</saml:AttributeStatement>\n" +
			"\t\t\t\t<saml:AttributeStatement id=\"UserLog\">\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"medcom:UserCivilRegistrationNumber\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>1903991234</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"medcom:UserGivenName\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>Jens</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"medcom:UserSurName\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>Hansen</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"medcom:UserEmailAddress\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>jh@nomail.dk</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"medcom:UserRole\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>PRAKTISERENDE_LAEGE</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"medcom:UserOccupation\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>Overlaege</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"medcom:UserAuthorizationCode\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>1234</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t</saml:AttributeStatement>\n" + "\t\t\t\t<saml:AttributeStatement id=\"SystemLog\">\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"medcom:ITSystemName\">\n"
			+ "\t\t\t\t\t\t<saml:AttributeValue>LaegeSystemet 3.0</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t\t<saml:Attribute Name=\"medcom:CareProviderID\"\n" + "\t\t\t\t\t\tNameFormat=\"medcom:ynumber\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>123456</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n"
			+ "\t\t\t\t\t<saml:Attribute Name=\"medcom:CareProviderName\">\n" +
			"\t\t\t\t\t\t<saml:AttributeValue>Hansens praksis</saml:AttributeValue>\n" + "\t\t\t\t\t</saml:Attribute>\n" +
			"\t\t\t\t</saml:AttributeStatement>\n" + "\t\t\t</saml:Assertion>\n" + "\t\t</wsse:Security>\n" +
			"\t\t<medcom:Header>\n"
			+ "\t\t\t<medcom:SecurityLevel>4</medcom:SecurityLevel>" + "\t\t\t<medcom:Linking>\n" +
			"\t\t\t\t<medcom:FlowID>aGQ5ZWxwcTA4N2ZubWM2ZA==</medcom:FlowID>\n" +
			"\t\t\t\t<medcom:MessageID>amRrMDk3d2doYXB2amY2cg==</medcom:MessageID>\n" + "\t\t\t</medcom:Linking>\n" +
			"\t\t\t<medcom:Priority>RUTINE</medcom:Priority>\n" + "\t\t</medcom:Header>\n"
			+ "\t</soap:Header>\n" + "\t<soap:Body/>\n" + "</soap:Envelope>";

		public const string OCES_CA_CRT =
			"LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlGR1RDQ0JBR2dBd0lCQWdJRVBraT" +
			"l4REFOQmdrcWhraUc5dzBCQVFVRkFEQXhNUXN3Q1FZRFZRUUdFd0pFDQpTekVNTUFvR0Ex" +
			"VUVDaE1EVkVSRE1SUXdFZ1lEVlFRREV3dFVSRU1nVDBORlV5QkRRVEFlRncwd016QXlNVE" +
			"V3DQpPRE01TXpCYUZ3MHpOekF5TVRFd09UQTVNekJhTURFeEN6QUpCZ05WQkFZVEFrUkxN"
			+ "UXd3Q2dZRFZRUUtFd05VDQpSRU14RkRBU0JnTlZCQU1UQzFSRVF5QlBRMFZUSUVOQk1JSU" +
			"JJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBUThBDQpNSUlCQ2dLQ0FRRUFyR0wyWVNDeXo4" +
			"REdoZGZqZWViTTdmSTVrcVNYTG1TamhGdUhuRXo5cFBQRVh5RzlWaERyDQoyeTVoN0pOcD" +
			"Q2UE12Wm5EQmZ3R3VNbzJIUDZRamtsTXhGYWFMMWE4ejNzTThXOUhwZzFEVGVMcEhUazB6"
			+ "WTBzDQoyUktZK2VQaHdVcDhoampFcWNSaGlOSmVyeG9tVGRYa29DSkhoTmxrdHhtVy9Pd1" +
			"o1TEtYSms1S1RNdVBKSXRVDQpHQnhJWVh2VmlHamFYYlhxelJvd3dZQ0RkbENxVDlIVTNU" +
			"anc3eGIwNFF4UUJyL3ErM3BKb1NnckhQYjhGVEtqDQpkR3FQcWNOaUtYRXg1VHVrWUJkZW" +
			"RPYmFFKzNwSHg4YjBiSm9jOFlRTkhWR0VCRGprQUIyUU11THQwTUpJZityDQpUcFBHV09t"
			+ "bGd0dDN4RHFac1hLVlNRVHd0eXY2ZTFtTzNRSURBUUFCbzRJQ056Q0NBak13RHdZRFZSMF" +
			"RBUUgvDQpCQVV3QXdFQi96QU9CZ05WSFE4QkFmOEVCQU1DQVFZd2dld0dBMVVkSUFTQjVE" +
			"Q0I0VENCM2dZSUtvRlFnU2tCDQpBUUV3Z2RFd0x3WUlLd1lCQlFVSEFnRVdJMmgwZEhBNk" +
			"x5OTNkM2N1WTJWeWRHbG1hV3RoZEM1a2F5OXlaWEJ2DQpjMmwwYjNKNU1JR2RCZ2dyQmdF"
			+ "RkJRY0NBakNCa0RBS0ZnTlVSRU13QXdJQkFScUJnVU5sY25ScFptbHJZWFJsDQpjaUJtY2" +
			"1FZ1pHVnVibVVnUTBFZ2RXUnpkR1ZrWlhNZ2RXNWtaWElnVDBsRUlERXVNaTR5TURndU1U" +
			"WTVMakV1DQpNUzR4TGlCRFpYSjBhV1pwWTJGMFpYTWdabkp2YlNCMGFHbHpJRU5CSUdGeV" +
			"pTQnBjM04xWldRZ2RXNWtaWElnDQpUMGxFSURFdU1pNHlNRGd1TVRZNUxqRXVNUzR4TGpB"
			+ "UkJnbGdoa2dCaHZoQ0FRRUVCQU1DQUFjd2dZRUdBMVVkDQpId1I2TUhnd1NLQkdvRVNrUW" +
			"pCQU1Rc3dDUVlEVlFRR0V3SkVTekVNTUFvR0ExVUVDaE1EVkVSRE1SUXdFZ1lEDQpWUVFE" +
			"RXd0VVJFTWdUME5GVXlCRFFURU5NQXNHQTFVRUF4TUVRMUpNTVRBc29DcWdLSVltYUhSMG" +
			"NEb3ZMMk55DQpiQzV2WTJWekxtTmxjblJwWm1scllYUXVaR3N2YjJObGN5NWpjbXd3S3dZ"
			+ "RFZSMFFCQ1F3SW9BUE1qQXdNekF5DQpNVEV3T0RNNU16QmFnUTh5TURNM01ESXhNVEE1TU" +
			"Rrek1Gb3dId1lEVlIwakJCZ3dGb0FVWUxXRjdGWmtmaElaDQpKMmNkVUJWTGM2NDcrUkl3" +
			"SFFZRFZSME9CQllFRkdDMWhleFdaSDRTR1NkbkhWQVZTM091Ty9rU01CMEdDU3FHDQpTSW" +
			"IyZlFkQkFBUVFNQTRiQ0ZZMkxqQTZOQzR3QXdJRWtEQU5CZ2txaGtpRzl3MEJBUVVGQUFP"
			+ "Q0FRRUFDcm9tDQpKa2JUYzZnSjgyc0xNSm45aXVGWGVoSFR1SlRYQ1JCdW83RTRBOUcyOG" +
			"tOQktXS25jdGo3ZkFYbU1YQW5WQmhPDQppbnhPNWRIS2pIaUl6eHZUa0l2bUkvZ0xEak5E" +
			"Zlp6aUNobVB5UUUrZEYxMHlZc2NBK1VZeUFGTVA4dVhCVjJZDQpjYWFZYjdaOHZUZC92dU" +
			"dUSlcxdjhBcXRGeGpoQTd3SEtjaXRKdWo0WWZEOUlRbCttbzZwYUgxSVluSzlBT29CDQpt"
			+ "YmdHZ2xHQlR2SDF0SkZVdVNONkFKcWZYWTNnUEdTNUdoS1NLc2VDUkhJNTNPSTh4dGhWOV" +
			"JWT3lBVU8yOGJRDQpZcWJzRmJTMUFvTGJySXlpZ2ZDYm1USDFJQ0NvaUdFS0I1K1UvTkRY" +
			"Rzh3dUYvTUVKM1puNjFTRC9hU1FmZ1k5DQpCS05ETGRyOEMyTHFMMTlpVXc9PQ0KLS0tLS" + "1FTkQgQ0VSVElGSUNBVEUtLS0tLQ0K";

		public const string ANOTHER_CERT =
			"MIIE3zCCBEigAwIBAgIBBTANBgkqhkiG9w0BAQQFADCByzELMAkGA1UEBhMCVVMx" +
			"EzARBgNVBAgTCkNhbGlmb3JuaWExEjAQBgNVBAcTCVN1bm55dmFsZTE9MDsGA1UE" +
			"ChM0WE1MIFNlY3VyaXR5IExpYnJhcnkgKGh0dHA6Ly93d3cuYWxla3NleS5jb20v" +
			"eG1sc2VjKTEZMBcGA1UECxMQUm9vdCBDZXJ0aWZpY2F0ZTEWMBQGA1UEAxMNQWxl"
			+ "a3NleSBTYW5pbjEhMB8GCSqGSIb3DQEJARYSeG1sc2VjQGFsZWtzZXkuY29tMB4X" +
			"DTAzMDMzMTA0MDIyMloXDTEzMDMyODA0MDIyMlowgb8xCzAJBgNVBAYTAlVTMRMw" +
			"EQYDVQQIEwpDYWxpZm9ybmlhMT0wOwYDVQQKEzRYTUwgU2VjdXJpdHkgTGlicmFy" +
			"eSAoaHR0cDovL3d3dy5hbGVrc2V5LmNvbS94bWxzZWMpMSEwHwYDVQQLExhFeGFt" +
			"cGxlcyBSU0EgQ2VydGlmaWNhdGUxFjAUBgNVBAMTDUFsZWtzZXkgU2FuaW4xITAf"
			+ "BgkqhkiG9w0BCQEWEnhtbHNlY0BhbGVrc2V5LmNvbTCCASIwDQYJKoZIhvcNAQEB" +
			"BQADggEPADCCAQoCggEBAJe4/rQ/gzV4FokE7CthjL/EXwCBSkXm2c3p4jyXO0Wt" +
			"quaNC3dxBwFPfPl94hmq3ZFZ9PHPPbp4RpYRnLZbRjlzVSOq954AXOXpSew7nD+E" +
			"mTqQrd9+ZIbGJnLOMQh5fhMVuOW/1lYCjWAhTCcYZPv7VXD2M70vVXDVXn6ZrqTg" +
			"qkVHE6gw1aCKncwg7OSOUclUxX8+Zi10v6N6+PPslFc5tKwAdWJhVLTQ4FKG+F53"
			+ "7FBDnNK6p4xiWryy/vPMYn4jYGvHUUk3eH4lFTCr+rSuJY8i/KNIf/IKim7g/o3w" +
			"Ae3GM8xrof2mgO8GjK/2QDqOQhQgYRIf4/wFsQXVZcMCAwEAAaOCAVcwggFTMAkG" +
			"A1UdEwQCMAAwLAYJYIZIAYb4QgENBB8WHU9wZW5TU0wgR2VuZXJhdGVkIENlcnRp" +
			"ZmljYXRlMB0GA1UdDgQWBBQkhCzy1FkgYosuXIaQo6owuicanDCB+AYDVR0jBIHw" +
			"MIHtgBS0ue+a5pcOaGUemM76VQ2JBttMfKGB0aSBzjCByzELMAkGA1UEBhMCVVMx"
			+ "EzARBgNVBAgTCkNhbGlmb3JuaWExEjAQBgNVBAcTCVN1bm55dmFsZTE9MDsGA1UE" +
			"ChM0WE1MIFNlY3VyaXR5IExpYnJhcnkgKGh0dHA6Ly93d3cuYWxla3NleS5jb20v" +
			"eG1sc2VjKTEZMBcGA1UECxMQUm9vdCBDZXJ0aWZpY2F0ZTEWMBQGA1UEAxMNQWxl" +
			"a3NleSBTYW5pbjEhMB8GCSqGSIb3DQEJARYSeG1sc2VjQGFsZWtzZXkuY29tggEA" +
			"MA0GCSqGSIb3DQEBBAUAA4GBALU/mzIxSv8vhDuomxFcplzwdlLZbvSQrfoNkMGY"
			+ "1UoS3YJrN+jZLWKSyWE3mIaPpElqXiXQGGkwD5iPQ1iJMbI7BeLvx6ZxX/f+c8Wn" +
			"ss0uc1NxfahMaBoyG15IL4+beqO182fosaKJTrJNG3mc//ANGU9OsQM9mfBEt4oL" + "NJ2D";

		public const string ENTRUST_ROOT =
			"MIIE2DCCBEGgAwIBAgIEN0rSQzANBgkqhkiG9w0BAQUFADCBwzELMAkGA1UEBhMC" +
			"VVMxFDASBgNVBAoTC0VudHJ1c3QubmV0MTswOQYDVQQLEzJ3d3cuZW50cnVzdC5u" +
			"ZXQvQ1BTIGluY29ycC4gYnkgcmVmLiAobGltaXRzIGxpYWIuKTElMCMGA1UECxMc" +
			"KGMpIDE5OTkgRW50cnVzdC5uZXQgTGltaXRlZDE6MDgGA1UEAxMxRW50cnVzdC5u"
			+ "ZXQgU2VjdXJlIFNlcnZlciBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTAeFw05OTA1" +
			"MjUxNjA5NDBaFw0xOTA1MjUxNjM5NDBaMIHDMQswCQYDVQQGEwJVUzEUMBIGA1UE" +
			"ChMLRW50cnVzdC5uZXQxOzA5BgNVBAsTMnd3dy5lbnRydXN0Lm5ldC9DUFMgaW5j" +
			"b3JwLiBieSByZWYuIChsaW1pdHMgbGlhYi4pMSUwIwYDVQQLExwoYykgMTk5OSBF" +
			"bnRydXN0Lm5ldCBMaW1pdGVkMTowOAYDVQQDEzFFbnRydXN0Lm5ldCBTZWN1cmUg"
			+ "U2VydmVyIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MIGdMA0GCSqGSIb3DQEBAQUA" +
			"A4GLADCBhwKBgQDNKIM0VBuJ8w+vN5Ex/68xYMmo6LIQaO2f55M28Qpku0f1BBc/" +
			"I0dNxScZgSYMVHINiC3ZH5oSn7yzcdOAGT9HZnuMNSjSuQrfJNqc1lB5gXpa0zf3" +
			"wkrYKZImZNHkmGw6AIr1NJtl+O3jEP/9uElY3KDegjlrgbEWGWG5VLbmQwIBA6OC" +
			"AdcwggHTMBEGCWCGSAGG+EIBAQQEAwIABzCCARkGA1UdHwSCARAwggEMMIHeoIHb"
			+ "oIHYpIHVMIHSMQswCQYDVQQGEwJVUzEUMBIGA1UEChMLRW50cnVzdC5uZXQxOzA5" +
			"BgNVBAsTMnd3dy5lbnRydXN0Lm5ldC9DUFMgaW5jb3JwLiBieSByZWYuIChsaW1p" +
			"dHMgbGlhYi4pMSUwIwYDVQQLExwoYykgMTk5OSBFbnRydXN0Lm5ldCBMaW1pdGVk" +
			"MTowOAYDVQQDEzFFbnRydXN0Lm5ldCBTZWN1cmUgU2VydmVyIENlcnRpZmljYXRp" +
			"b24gQXV0aG9yaXR5MQ0wCwYDVQQDEwRDUkwxMCmgJ6AlhiNodHRwOi8vd3d3LmVu"
			+ "dHJ1c3QubmV0L0NSTC9uZXQxLmNybDArBgNVHRAEJDAigA8xOTk5MDUyNTE2MDk0" +
			"MFqBDzIwMTkwNTI1MTYwOTQwWjALBgNVHQ8EBAMCAQYwHwYDVR0jBBgwFoAU8Bdi" +
			"E1U9s/8KAGv7UISX8+1i0BowHQYDVR0OBBYEFPAXYhNVPbP/CgBr+1CEl/PtYtAa" +
			"MAwGA1UdEwQFMAMBAf8wGQYJKoZIhvZ9B0EABAwwChsEVjQuMAMCBJAwDQYJKoZI" +
			"hvcNAQEFBQADgYEAkNwwAvpkdMKnCqV8IY00F6j7Rw7/JXyNEwr75Ji174z4xRAN"
			+ "95K+8cPV1ZVqBLssziY2ZcgxxufuP+NXdYR6Ee9GTxj005i7qIcyunL2POI9n9cd" +
			"2cNgQ4xYDiKWL2KjLB+6rQXvqzJ4h6BUcxm1XAX5Uj5tLUUL9wqT6u0G+bI=";

		public const string ENTRUSTROOT_TDCROOT =
			"MIIEJTCCA46gAwIBAgIERp8GBjANBgkqhkiG9w0BAQUFADCBwzELMAkGA1UEBhMC" +
			"VVMxFDASBgNVBAoTC0VudHJ1c3QubmV0MTswOQYDVQQLEzJ3d3cuZW50cnVzdC5u" +
			"ZXQvQ1BTIGluY29ycC4gYnkgcmVmLiAobGltaXRzIGxpYWIuKTElMCMGA1UECxMc" +
			"KGMpIDE5OTkgRW50cnVzdC5uZXQgTGltaXRlZDE6MDgGA1UEAxMxRW50cnVzdC5u"
			+ "ZXQgU2VjdXJlIFNlcnZlciBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTAeFw0wOTEw" +
			"MjIxNDA3MjJaFw0xNDA5MDYwNDAwMDBaMEMxCzAJBgNVBAYTAkRLMRUwEwYDVQQK" +
			"EwxUREMgSW50ZXJuZXQxHTAbBgNVBAsTFFREQyBJbnRlcm5ldCBSb290IENBMIIB" +
			"IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxLhAvJHVYx/XmaCLDEAedLdI" +
			"nUaMArLgJF/wGROnN4NrXceO+YQwzho7+vvOi20jxsNuZp+Jpd/gQlBn+h9sHvTQ"
			+ "Bda/ytZO5GhgbEaqHF1j4QeGDmUApy6mcca8uYGoOn0a0vnRrEvLznWv3Hv6gXPU" +
			"/Lq9QYjUdLP5Xjg6PEOo0pVOd20TDJ2PeAG3WiAfAzc14izbSysseLlJ28TQx5yc" +
			"5IogCSEWVmb/Bexb4/DPqyQkXsN/cHoSxNK1EKC2IeGNeGlVRGn1ypYcNIUXJXfi" +
			"9i8nmHj9eQY6otZaQ8H/7AQ77hPv01ha/5Lr7K7a8jcDR0G2l8ktCkEiu7vmpwID" +
			"AQABo4IBHzCCARswDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQEw"
			+ "MwYIKwYBBQUHAQEEJzAlMCMGCCsGAQUFBzABhhdodHRwOi8vb2NzcC5lbnRydXN0" +
			"Lm5ldDAzBgNVHR8ELDAqMCigJqAkhiJodHRwOi8vY3JsLmVudHJ1c3QubmV0L3Nl" +
			"cnZlcjEuY3JsMEsGA1UdIAREMEIwQAYLKwYBBAGiIgIBAQIwMTAvBggrBgEFBQcC" +
			"ARYjaHR0cDovL3d3dy5jZXJ0aWZpa2F0LmRrL3JlcG9zaXRvcnkwHwYDVR0jBBgw" +
			"FoAU8BdiE1U9s/8KAGv7UISX8+1i0BowHQYDVR0OBBYEFGxkAcf9hW2syNqeUAiF"
			+ "CLU8VqhQMA0GCSqGSIb3DQEBBQUAA4GBAMytotU9jNwG97XAJ92JBAJYIJgzTt97" +
			"F7e0w/Vqdv51GKolM5/bOGlNdu2aLgD2R7QL3cKICPtHp/zq/Qsk3Ef9m60Qf1A/" +
			"RwrRbPkQWxbXYr0gDRfEz68tYD9lQ+aptUnCL/wsabhaJWL09/vkl+vdLtj9c3zG" + "EjRaKPaJ+Fmt";

		public const string TDCROOT_TDCSSL_2 =
			"MIIECjCCAvKgAwIBAgIEPBoncjANBgkqhkiG9w0BAQUFADBDMQswCQYDVQQGEwJE" +
			"SzEVMBMGA1UEChMMVERDIEludGVybmV0MR0wGwYDVQQLExRUREMgSW50ZXJuZXQg" +
			"Um9vdCBDQTAeFw0xMDAzMjUwODUxMjlaFw0xNDA4MjUwOTIxMjlaMDcxCzAJBgNV" +
			"BAYTAkRLMQwwCgYDVQQKEwNUREMxGjAYBgNVBAsTEVREQyBTU0wgU2VydmVyIENB"
			+ "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2ccg54uj7AKBZCwhFQbn" +
			"0ovjkDjjFw2pi1eMHlWqlHLm6dUMtfuL77fIkNUAFSurGfMFL1xoaXVaq5z4c7gC" +
			"G2pEkHdg3F4RHAOv6JvpbMDBRFLyNUgC6x9tk4YG9qGsGtDTljAT+ATKorFPszho" +
			"CP5SAKOGgnMY/MGoxYhOFjjc5+PfpqZNO5nG/FbzzB+lwrgEuwi6odMA92/2Zgi1" +
			"xRr0AxfnhkZPfKU9XHrLEsaPnk3DH2gXf1q++h4YMSwWX7Kqp+ffKA2wIIeKOZ33"
			+ "bXNyMXjgi6EYQyALjCpZCdZX4ok9DSUEx1WXOy2AOrKMcMTF1vvJOxAQOJthyq0E" +
			"ewIDAQABo4IBEDCCAQwwgZMGA1UdHwSBizCBiDBaoFigVqRUMFIxCzAJBgNVBAYT" +
			"AkRLMRUwEwYDVQQKEwxUREMgSW50ZXJuZXQxHTAbBgNVBAsTFFREQyBJbnRlcm5l" +
			"dCBSb290IENBMQ0wCwYDVQQDEwRDUkwxMCqgKKAmhiRodHRwOi8vY3JsLmNlcnRp" +
			"ZmlrYXQuZGsvUm9vdF9DQS5jcmwwCwYDVR0PBAQDAgEGMB8GA1UdIwQYMBaAFGxk"
			+ "Acf9hW2syNqeUAiFCLU8VqhQMB0GA1UdDgQWBBT9HsKzCDqV0dSlh87NQYRz7zN0" +
			"DTAMBgNVHRMEBTADAQH/MBkGCSqGSIb2fQdBAAQMMAobBFY2LjADAgSQMA0GCSqG" +
			"SIb3DQEBBQUAA4IBAQAU8G3+r8x8msjQ6xj6io0p7Jmr2mGXC/ORhZf4QdluWaWQ" +
			"aNIjvuKOdXae6jlTc/BfmXv2PzjME3040loxdvs4SkiH9YO3wU4+YCxtoe2ngvt7" +
			"XkEDKb9Z6SJqOL/TDNsLM6SWcIkGtiEWfevwVwBh0tZHN7s5pqUtNhvoyXeijKN3"
			+ "LDDKC07vwKllqS0zW9fkeZ0LxyxqVmcXAkH+/Yyd333bEQM2CtgxVZekkX1xFdjF" +
			"bRCsImNsMAyHOu258nPNJ1rvbB69F3r2pNGxznven3INy9bbsvHihDwYGJVDshyu" + "50FxBsiYOB6e1zFqDr0OPLoSjOHq1gwXwFI8Z5jZ";

		// SSR: CVR:34051178-UID:1336055110451
		private const string OldNemloginIdpCertificateString =
				"MIIFrTCCBRagAwIBAgIEQDgeoDANBgkqhkiG9w0BAQUFADA/MQswCQYDVQQGEwJESzEMMAoGA1UE" +
				"ChMDVERDMSIwIAYDVQQDExlUREMgT0NFUyBTeXN0ZW10ZXN0IENBIElJMB4XDTEyMDUwNzEzMDIz" +
				"N1oXDTE0MDUwNzEzMzIzN1owgaIxCzAJBgNVBAYTAkRLMTEwLwYDVQQKEyhEaWdpdGFsaXNlcmlu" +
				"Z3NzdHlyZWxzZW4gLy8gQ1ZSOjM0MDUxMTc4MWAwJQYDVQQFEx5DVlI6MzQwNTExNzgtVUlEOjEz" +
				"MzYwNTUxMTA0NTEwNwYDVQQDEzBEaWdpdGFsaXNlcmluZ3NzdHlyZWxzZW4gLSBEaWdzdCAtIE5l" +
				"bUxvZy1pbiBUZXMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDnV90kgwkzKmuCtfAb" +
				"2OORg6DufFoLekR1jDfRyf8qYIkAGEELYiNuatR8o29/gEWibNy/W9i0cMNH44e9t6cPf6GXqqQl" +
				"FxbV6SWuX4LPMsq7kJiWw+kkV3Ac66jsKVEJduSxHRKfNiFq3Fg4AAFn7TsvQSAapHZXUwMpTNXf" +
				"qq6BkO9RvJjkMlj3TpDGIgiRMWyQmH84RsL/SjgiLDbkesCToWr8GylQVCascIfihwxxDNM3QJmz" +
				"t7oZuYX1+uZkQayq57vZxqGldvnhTuAjCNt7nN76Doy584D7kzPU2vrad0sNUrcvPzzHQBj2aKgw" +
				"uhColBOtLOjcELtKn0C7AgMBAAGjggLMMIICyDAOBgNVHQ8BAf8EBAMCA7gwKwYDVR0QBCQwIoAP" +
				"MjAxMjA1MDcxMzAyMzdagQ8yMDE0MDUwNzEzMzIzN1owRgYIKwYBBQUHAQEEOjA4MDYGCCsGAQUF" +
				"BzABhipodHRwOi8vdGVzdC5vY3NwLmNlcnRpZmlrYXQuZGsvb2NzcC9zdGF0dXMwggEDBgNVHSAE" +
				"gfswgfgwgfUGCSkBAQEBAQEBAzCB5zAvBggrBgEFBQcCARYjaHR0cDovL3d3dy5jZXJ0aWZpa2F0" +
				"LmRrL3JlcG9zaXRvcnkwgbMGCCsGAQUFBwICMIGmMAoWA1REQzADAgEBGoGXVERDIFRlc3QgQ2Vy" +
				"dGlmaWthdGVyIGZyYSBkZW5uZSBDQSB1ZHN0ZWRlcyB1bmRlciBPSUQgMS4xLjEuMS4xLjEuMS4x" +
				"LjEuMy4gVERDIFRlc3QgQ2VydGlmaWNhdGVzIGZyb20gdGhpcyBDQSBhcmUgaXNzdWVkIHVuZGVy" +
				"IE9JRCAxLjEuMS4xLjEuMS4xLjEuMS4zLjAcBglghkgBhvhCAQ0EDxYNb3JnYW5XZWJOb0RpcjAc" +
				"BgNVHREEFTATgRFuZW1sb2dpbkBkaWdzdC5kazCBlwYDVR0fBIGPMIGMMFegVaBTpFEwTzELMAkG" +
				"A1UEBhMCREsxDDAKBgNVBAoTA1REQzEiMCAGA1UEAxMZVERDIE9DRVMgU3lzdGVtdGVzdCBDQSBJ" +
				"STEOMAwGA1UEAxMFQ1JMMzIwMaAvoC2GK2h0dHA6Ly90ZXN0LmNybC5vY2VzLmNlcnRpZmlrYXQu" +
				"ZGsvb2Nlcy5jcmwwHwYDVR0jBBgwFoAUHJgJRxpMOLkQxQQpW/H0ToBqzH4wHQYDVR0OBBYEFKhW" +
				"lVALv2E2JYC8SQPu2jvRXVjqMAkGA1UdEwQCMAAwGQYJKoZIhvZ9B0EABAwwChsEVjcuMQMCA6gw" +
				"DQYJKoZIhvcNAQEFBQADgYEAQlw2hwwrFX9rehfxsMDkiOJGLJcVmSpK9bA/qDUCr8EU4aBfBE+o" +
				"IHaAQxmrG9Rsvyhf1rUE+Cir2GQsS5gcG9zIhwEwtCitQutM7dOJ1eIbApmHF8bHike8eouerTcr" +
				"oqM6EY8TbfEkDEtDuQxk+GqbCh5gpKg243ZLZiG3Aes=";

		//SERIALNUMBER=CVR:34051178-UID:83384970 + CN=Digitaliseringsstyrelsen - NemLog-in Test, O=Digitaliseringsstyrelsen // CVR:34051178, C=DK
		private const string NewNemloginIdpCertificateString =
				"MIIGRTCCBS2gAwIBAgIEUw8DszANBgkqhkiG9w0BAQsFADBHMQswCQYDVQQGEwJESzESMBAGA1UE" +
				"CgwJVFJVU1QyNDA4MSQwIgYDVQQDDBtUUlVTVDI0MDggU3lzdGVtdGVzdCBYSVggQ0EwHhcNMTQw" +
				"NTA1MTMzNTU4WhcNMTcwNTA1MTMzNTEyWjCBljELMAkGA1UEBhMCREsxMTAvBgNVBAoMKERpZ2l0" +
				"YWxpc2VyaW5nc3N0eXJlbHNlbiAvLyBDVlI6MzQwNTExNzgxVDAgBgNVBAUTGUNWUjozNDA1MTE3" +
				"OC1VSUQ6ODMzODQ5NzAwMAYDVQQDDClEaWdpdGFsaXNlcmluZ3NzdHlyZWxzZW4gLSBOZW1Mb2ct" +
				"aW4gVGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALnCmDRMztjDckSupQBLcEzr" +
				"RRJnAFxzEFdB7Cj6ApMQ/YxqKzfL/TSIr3v2mdgQNnsJGz91YbAteDPRHR/K1W3kqoIX/qH2uXDz" +
				"HK+qi4YD9D8s4MnHAt02x6t0TgKQGjn1XO6lgLQ563DjtgD2fdPm9USV2Lkxe5ofNRG7yvWowBWj" +
				"XKia8D64k6zSzoHKdPz6GCy9S0NmwIyJE0sJavcfwxT3/ia0g63/xD77SteT4H/OR/DLis7FLnfk" +
				"Lp8yrd5xAk4nEGizmjrg2OVJmIMMPK6PQdw+/lqSdgaPDxMD6yoIwWshux5Rup1+piMLg852odHR" +
				"6EhUzjEsi9DnWWcCAwEAAaOCAucwggLjMA4GA1UdDwEB/wQEAwIEsDCBlwYIKwYBBQUHAQEEgYow" +
				"gYcwPAYIKwYBBQUHMAGGMGh0dHA6Ly9vY3NwLnN5c3RlbXRlc3QxOS50cnVzdDI0MDguY29tL3Jl" +
				"c3BvbmRlcjBHBggrBgEFBQcwAoY7aHR0cDovL3YuYWlhLnN5c3RlbXRlc3QxOS50cnVzdDI0MDgu" +
				"Y29tL3N5c3RlbXRlc3QxOS1jYS5jZXIwggEgBgNVHSAEggEXMIIBEzCCAQ8GDSsGAQQBgfRRAgQG" +
				"AwQwgf0wLwYIKwYBBQUHAgEWI2h0dHA6Ly93d3cudHJ1c3QyNDA4LmNvbS9yZXBvc2l0b3J5MIHJ" +
				"BggrBgEFBQcCAjCBvDAMFgVEYW5JRDADAgEBGoGrRGFuSUQgdGVzdCBjZXJ0aWZpa2F0ZXIgZnJh" +
				"IGRlbm5lIENBIHVkc3RlZGVzIHVuZGVyIE9JRCAxLjMuNi4xLjQuMS4zMTMxMy4yLjQuNi4zLjQu" +
				"IERhbklEIHRlc3QgY2VydGlmaWNhdGVzIGZyb20gdGhpcyBDQSBhcmUgaXNzdWVkIHVuZGVyIE9J" +
				"RCAxLjMuNi4xLjQuMS4zMTMxMy4yLjQuNi4zLjQuMBwGA1UdEQQVMBOBEW5lbWxvZ2luQGRpZ3N0" +
				"LmRrMIGpBgNVHR8EgaEwgZ4wPKA6oDiGNmh0dHA6Ly9jcmwuc3lzdGVtdGVzdDE5LnRydXN0MjQw" +
				"OC5jb20vc3lzdGVtdGVzdDE5LmNybDBeoFygWqRYMFYxCzAJBgNVBAYTAkRLMRIwEAYDVQQKDAlU" +
				"UlVTVDI0MDgxJDAiBgNVBAMMG1RSVVNUMjQwOCBTeXN0ZW10ZXN0IFhJWCBDQTENMAsGA1UEAwwE" +
				"Q1JMMjAfBgNVHSMEGDAWgBTMAlUM5IF0ryBU1REUV5yRUjh/oDAdBgNVHQ4EFgQUwm9c3oUHE/zZ" +
				"/43g4RUhswnMVAowCQYDVR0TBAIwADANBgkqhkiG9w0BAQsFAAOCAQEACLh3Ovvljv4b/Ywf/8Wx" +
				"oB2y50Oqt8rpwXZp+no4d5tLqIMTSAlQxL0lAf4Qm4e6tF5m/55+dLwxw5/Dqwa0bQXHt98vJjSB" +
				"YLQH6rwfDzNmVGimO1n84k4MMYY449ykjqRNDfCS3+5+zV/An4CH9eUhvB0AHHWbD6eALw39sPGx" +
				"c5kHADTOdJ5SboSm9DHYdLLt9k8HyxrHIkcJApLWPgyFmkE0+8jtuQQluN62F5+j5d53oTKinHEd" +
				"7adM0ea537vNf5uBGu6h9OTXlhZwM9tlnrsTYQTTAIzdxGPlpD9Zvo5nmJHwILdRonm8rZf3vAm5" +
				"9/U6+Cht4+X2l5zxyg==";

		public const string MOCES_TEST_PFX_RESOURCE = "/oces2/PP/MOCES_gyldig.p12";

		public const string MOCES_TEST_PFX_PWD = "Test1234";

		public const string TEST_ROOT_CERT_RESOURCE = "/TDCOCESSTEST2.cer";

		public static InMemoryCredentialVault GetCredentialVault()
		{
			return GetCredentialVaultFromResource("oces2/PP/MOCES_cpr_gyldig.p12");
		}

		public static InMemoryCredentialVault GetVocesCredentialVault()
		{
			var vault = GetCredentialVaultFromResource("oces2/PP/VOCES_gyldig.p12");
			return vault;
		}

		public static ICredentialVault GetVoces3CredentialVault()
		{
			return GetCredentialVaultFromResource("oces3/devtest4/VOCES_gyldig.p12");
		}
		public static ICredentialVault GetFoces3CredentialVault()
		{
			return GetCredentialVaultFromResource("oces3/devtest4/FOCES_gyldig.p12");
		}

		public static ICredentialVault GetMoces3CredentialVault()
		{
			return GetCredentialVaultFromResource("oces3/devtest4/MOCES_cpr_gyldig.p12");
		}

		public static InMemoryCredentialVault GetOldIdpTestCredentialVault()
		{
			return GetCredentialVaultFromBase64String(OldNemloginIdpCertificateString, "OldIdp");
		}

		public static InMemoryCredentialVault GetNewIdpTestCredentialVault()
		{
			return GetCredentialVaultFromBase64String(NewNemloginIdpCertificateString, "NewIdp");
		}

		public static InMemoryCredentialVault GetCredentialVaultFromResource(string resourceName, string password = "Test1234")
		{
			var systemCredentialCert = new X509Certificate2(TestContext.CurrentContext.TestDirectory +
														"/Resources/" + resourceName, password);
			var credentialVault = new InMemoryCredentialVault(systemCredentialCert);
			return credentialVault;
		}

        public static InMemoryCredentialVault GetCredentialVault(string certificatePath, string password)
        {
            var certificate = new X509Certificate2(certificatePath, password);
            var credentialVault = new InMemoryCredentialVault(certificate);
            return credentialVault;

        }

        public static InMemoryCredentialVault GetCredentialVaultFromBase64String(string certificate, string postFix)
		{
			var systemCredentialCert = new X509Certificate2(Convert.FromBase64String(certificate));
			var credentialVault = new InMemoryCredentialVault(systemCredentialCert);
			return credentialVault;
		}

        public static InMemoryCredentialVault GetOCES2CredentialVault() => GetCredentialVaultFromResource("voces2.pkcs12", "1234Test");

        public static InMemoryCredentialVault getCredentialVaultCertInLdap() => GetVocesCredentialVault();
    }
}
