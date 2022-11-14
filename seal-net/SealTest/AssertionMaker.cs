using System;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.fmk;
using dk.nsi.seal;
using dk.nsi.seal.Constants;
using SealTest.Certificate;
using Attribute = dk.nsi.fmk.Attribute;
using KeyInfo = dk.nsi.fmk.KeyInfo;

namespace SealTest
{
    public class AssertionMaker
    {
        public static Assertion MakeAssertionForSTS(ICertificate certificate, int authenticationLevel, string cpr = "1802602810", string authorization = "ZXCVB") =>
            MakeAssertionForSTS(certificate.Certificate, authenticationLevel, certificate.Cvr, cpr, authorization);

        public static Assertion MakeAssertionForSTS(X509Certificate2 certificate, int authenticationLevel, string cvr, string cpr = "1802602810", string authorization = "ZXCVB")
        {
            var vnow = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5);

            var ass = new Assertion
            {
                IssueInstant = vnow,
                id = "IDCard",
                Version = 2.0m,
                Issuer = "WinPLC",
                Conditions = new Conditions
                {
                    NotBefore = vnow,
                    NotOnOrAfter = vnow + TimeSpan.FromHours(8)
                },
                Subject = new Subject
                {
                    NameID = new NameID
                    {
                        Format = "http://rep.oio.dk/cpr.dk/xml/schemas/core/2005/03/18/CPR_PersonCivilRegistrationIdentifier.xsd",
                        Value = "2203333571"
                    },
                    SubjectConfirmation = new SubjectConfirmation
                    {
                        ConfirmationMethod = ConfirmationMethod.urnoasisnamestcSAML20cmholderofkey,
                        SubjectConfirmationData = new SubjectConfirmationData
                        {
                            Item = new KeyInfo
                            {
                                Item = "OCESSignature"
                            }
                        }
                    }
                },
                AttributeStatement = new[]
                {
                    new AttributeStatement
                    {
                        id = AttributeStatementID.IDCardData,
                        Attribute = new[]
                        {
                            new Attribute {Name = SosiAttributes.IDCardID, AttributeValue = Guid.NewGuid().ToString("D")},
                            new Attribute {Name = SosiAttributes.IDCardVersion, AttributeValue = "1.0.1"},
                            new Attribute {Name = SosiAttributes.IDCardType, AttributeValue = "user"},
                            new Attribute {Name = SosiAttributes.AuthenticationLevel, AttributeValue = "" + authenticationLevel}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.UserLog,
                        Attribute = new[]
                        {
                            new Attribute {Name = MedComAttributes.UserCivilRegistrationNumber, AttributeValue = cpr},
                            new Attribute {Name = MedComAttributes.UserGivenName, AttributeValue = "Stine"},
                            new Attribute {Name = MedComAttributes.UserSurname, AttributeValue = "Svendsen"},
                            new Attribute {Name = MedComAttributes.UserEmailAddress, AttributeValue = "stineSvendsen@example.com"},
                            new Attribute {Name = MedComAttributes.UserRole, AttributeValue = "7170"},
                            new Attribute {Name = MedComAttributes.UserAuthorizationCode, AttributeValue = authorization}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.SystemLog,
                        Attribute = new[]
                        {
                            new Attribute {Name = MedComAttributes.ItSystemName, AttributeValue = "Sygdom.dk"},
                            new Attribute {Name = MedComAttributes.CareProviderId, AttributeValue = cvr, NameFormat = "medcom:cvrnumber"},
                            new Attribute {Name = MedComAttributes.CareProviderName, AttributeValue = "Statens Serum Institut"}
                        }
                    }
                }
            };

            return certificate == null ? ass : SealUtilities.SignAssertion(ass, certificate);
        }

        public static Assertion MakeAssertion()
        {
            var vnow = DateTimeEx.UtcNowRound - TimeSpan.FromMinutes(5);

            return new Assertion
            {
                IssueInstant = vnow,
                id = "IDCard",
                Version = 2.0m,
                Issuer = "TESTSTS",
                Conditions = new Conditions
                {
                    NotBefore = vnow,
                    NotOnOrAfter = vnow + TimeSpan.FromHours(8)
                },
                Subject = new Subject
                {
                    NameID = new NameID
                    {
                        Format = "medcom:cprnumber",
                        Value = "2408631478"
                    },
                    SubjectConfirmation = new SubjectConfirmation
                    {
                        ConfirmationMethod = ConfirmationMethod.urnoasisnamestcSAML20cmholderofkey,
                        SubjectConfirmationData = new SubjectConfirmationData
                        {
                            Item = new KeyInfo
                            {
                                Item = "OCESSignature"
                            }
                        }
                    }
                },
                AttributeStatement = new[]
                {
                    new AttributeStatement
                    {
                        id = AttributeStatementID.IDCardData,
                        Attribute = new[]
                        {
                            new Attribute {Name = SosiAttributes.IDCardID, AttributeValue = Guid.NewGuid().ToString("D")},
                            new Attribute {Name = SosiAttributes.IDCardVersion, AttributeValue = "1.0.1"},
                            new Attribute {Name = SosiAttributes.IDCardType, AttributeValue = "user"},
                            new Attribute {Name = SosiAttributes.AuthenticationLevel, AttributeValue = "4"},
                            new Attribute {Name = SosiAttributes.OcesCertHash, AttributeValue = Global.cert.GetCertHashString()}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.UserLog,
                        Attribute = new[]
                        {
                            new Attribute {Name = MedComAttributes.UserCivilRegistrationNumber, AttributeValue = "2408631478"},
                            new Attribute {Name = MedComAttributes.UserGivenName, AttributeValue = "Amaja Christiansen"},
                            new Attribute {Name = MedComAttributes.UserSurname, AttributeValue = "-"},
                            new Attribute {Name = MedComAttributes.UserEmailAddress, AttributeValue = "jso@trifork.com"},
                            new Attribute {Name = MedComAttributes.UserRole, AttributeValue = "5175"},
                            new Attribute {Name = MedComAttributes.UserAuthorizationCode, AttributeValue = "5GXFR"}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.SystemLog,
                        Attribute = new[]
                        {
                            new Attribute {Name = MedComAttributes.ItSystemName, AttributeValue = "Sygdom.dk"},
                            new Attribute {Name = MedComAttributes.CareProviderId, AttributeValue = "25520041", NameFormat = "medcom:cvrnumber"},
                            new Attribute {Name = MedComAttributes.CareProviderName, AttributeValue = "TRIFORK SERVICES A/S // CVR:25520041"}
                        }
                    }
                }
            };
        }
    }
}
