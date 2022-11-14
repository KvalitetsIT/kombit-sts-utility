﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal.Vault;

namespace SealTest.Certificate
{
    public class CertificateOces2FocesRevoked : ICertificate
    {
        public X509Certificate2 Certificate => new(CertificatePath, CertificatePassword);
        public string CertificatePath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Resources","oces2","PP","FOCES_spaerret.p12");
        public string CertificatePassword => "Test1234";

        public ICredentialVault CredentialVault => throw new NotImplementedException();
        public string Cvr => "30808460";

        public string Cpr => throw new NotImplementedException();
    }
}
