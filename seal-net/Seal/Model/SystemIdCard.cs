using System;
using System.Xml.Linq;
using dk.nsi.seal.dgwstypes;
using Attribute = dk.nsi.seal.dgwstypes.Attribute;
using System.Xml.Serialization;
using System.Xml;

namespace dk.nsi.seal
{
	public class SystemIdCard : IdCard
    {
		[XmlElement]
		public SystemInfo SystemInfo { get; protected set; }

		/// <summary>
		/// Empty constructor ONLY for Serialization/Deserialization
		/// </summary>
		public SystemIdCard() : base() { }

        public SystemIdCard(IdCard toCopy, string issuer, string certHash, string alternativeIdentifier, SystemInfo systemInfo, AuthenticationLevel authLevel) : base(toCopy, issuer, certHash, alternativeIdentifier, authLevel)
        {
            SystemInfo = systemInfo;
        }

        public SystemIdCard(string version, AuthenticationLevel authLevel, string issuer, SystemInfo systemInfo, string certHash, string alternativeIdentifier, string userName, string password) : base(version, authLevel, issuer, certHash, alternativeIdentifier, userName, password)
        {
            ModelUtilities.ValidateNotEmpty(systemInfo.ItSystemName, "SystemInfo must be specified");
            ModelUtilities.ValidateNotNull(systemInfo.CareProvider, "SystemInfo must be specified");
            SystemInfo = systemInfo;
        }

        public SystemIdCard(string version, XElement xAssertion, string cardId, AuthenticationLevel authLevel, string certHash, string issuer, SystemInfo systemInfo, DateTime creationDate, DateTime expiryDate, string alternativeIdentifier, string username, string password) : base(version, xAssertion, cardId, authLevel, certHash, issuer, creationDate, expiryDate, alternativeIdentifier, username, password)
        {
            ModelUtilities.ValidateNotNull(systemInfo, "SystemInfo must be specified");
            ModelUtilities.ValidateNotNull(systemInfo.CareProvider, "SystemInfo must be specified");
            this.SystemInfo = systemInfo;
        }

        protected override Assertion GenerateAssertion()
        {
            //Create SubjectConfirmationData based on AuthLevel.
            SubjectConfirmation subjectConf = new SubjectConfirmation();
            if (AuthenticationLevel.Equals(AuthenticationLevel.UsernamePasswordAuthentication))
            {
                var subjectConfData = new SubjectConfirmationData
                {
                    Item = new UsernameToken() { Username = Username, Password = Password }
                };
                subjectConf.SubjectConfirmationData = subjectConfData;
            }
            else if (AuthenticationLevel.Equals(AuthenticationLevel.MocesTrustedUser) || AuthenticationLevel.Equals(AuthenticationLevel.VocesTrustedSystem))
            {
                var subjectConfData = new SubjectConfirmationData
                {
                    Item = new KeyInfo
                    {
                        Item = "OCESSignature"
                    }
                };
                subjectConf.SubjectConfirmationData = subjectConfData;
                subjectConf.ConfirmationMethod = ConfirmationMethod.urnoasisnamestcSAML20cmholderofkey;
            }

            //Create NameID based on alternative identifier
            NameID nameId = new NameID();
            if(string.IsNullOrEmpty(AlternativeIdentifier))
            {
                nameId.Format = SystemInfo.CareProvider.Type;
                nameId.Value = SystemInfo.CareProvider.Id;
            }
            else
            {
                nameId.Format = SubjectIdentifierType.medcomother;
                nameId.Value = AlternativeIdentifier;
            }

            var ass = new Assertion
            {
                IssueInstant = CreatedDate,
                id = "IDCard",
                Version = 2.0m,
                Issuer = Issuer,
                Conditions = new Conditions
                {
                    NotBefore = CreatedDate,
                    NotOnOrAfter = ExpiryDate
                },
                Subject = new Subject
                {
                    NameID = nameId,
                    SubjectConfirmation = AuthenticationLevel.Equals(AuthenticationLevel.NoAuthentication) ? null : subjectConf
                },
                AttributeStatement = new[]
                {
                    new AttributeStatement
                    {
                        id = AttributeStatementID.IDCardData,
                        Attribute = new []
                        {
                            new Attribute {Name = AttributeName.sosiIDCardID, AttributeValue = IdCardId},
                            new Attribute {Name = AttributeName.sosiIDCardVersion, AttributeValue = Version},
                            new Attribute {Name = AttributeName.sosiIDCardType, AttributeValue = "system"},
                            new Attribute {Name = AttributeName.sosiAuthenticationLevel, AttributeValue = AuthenticationLevel.Level.ToString()},
                            new Attribute {Name = AttributeName.sosiOCESCertHash, AttributeValue = CertHash,}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.SystemLog,
                        Attribute = new []
                        {
                            new Attribute {Name = AttributeName.medcomITSystemName, AttributeValue = SystemInfo.ItSystemName},
                            new Attribute
                            {
                                Name = AttributeName.medcomCareProviderID,
                                AttributeValue = SystemInfo.CareProvider.Id,
                                NameFormatSpecified = true,
                                NameFormat = SystemInfo.CareProvider.Type
                            },
                            new Attribute {Name = AttributeName.medcomCareProviderName, AttributeValue = SystemInfo.CareProvider.OrgName},
                        }
                    }
                }
            };
            return ass;
        }

	    public override bool Equals(object obj)
	    {
			var otherSystemIdCard = obj as SystemIdCard;
			if (otherSystemIdCard == null)
				return false;

		    var result = SystemInfo.Equals(otherSystemIdCard.SystemInfo);
		    return result;
	    }

		public override void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();

			reader.ReadStartElement();
			{
				LastDomOperation = reader.GetAttribute("LastDomOperation");
				string needsSignatureAttribute = reader.GetAttribute("NeedsSignature");
				if (!string.IsNullOrEmpty(needsSignatureAttribute))
					NeedsSignature = bool.Parse(needsSignatureAttribute);
				AlternativeIdentifier = reader.GetAttribute("AlternativeIdentifier");
				CertHash = reader.GetAttribute("CertHash");
				IdCardId = reader.GetAttribute("IdCardId");
				Issuer = reader.GetAttribute("Issuer");
				Password = reader.GetAttribute("Password");
				Username = reader.GetAttribute("Username");
				Version = reader.GetAttribute("Version");
				var authenticationLevelAttribute = reader.GetAttribute("AuthenticationLevel");
				if (!string.IsNullOrEmpty(authenticationLevelAttribute))
					AuthenticationLevel = AuthenticationLevel.GetEnumeratedValue(int.Parse(authenticationLevelAttribute));

				while (reader.Read())
				{
					if (reader.IsStartElement())
					{
						if (reader.Name == "CreatedDate")
						{
							reader.ReadStartElement();
							this.CreatedDate = (DateTime)new XmlSerializer(typeof(DateTime)).Deserialize(reader);
							reader.ReadEndElement();
						}
						if (reader.Name == "ExpiryDate")
						{
							reader.ReadStartElement();
							this.ExpiryDate = (DateTime)new XmlSerializer(typeof(DateTime)).Deserialize(reader);
							reader.ReadEndElement();
						}
						if (reader.Name == "Xassertion")
						{
							reader.ReadStartElement();
							this.Xassertion = XElement.Parse(reader.ReadOuterXml(), LoadOptions.PreserveWhitespace);
						}
						if (reader.Name == "SystemInfo")
						{
							this.SystemInfo = new SystemInfo(null, null);
							(this.SystemInfo as IXmlSerializable).ReadXml(reader);
						}
					}
				}
			}
		}

		public override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("UserIdCard");
			{
				writer.WriteAttributeString("LastDomOperation", LastDomOperation);
				writer.WriteAttributeString("NeedsSignature", NeedsSignature.ToString());
				writer.WriteAttributeString("AlternativeIdentifier", AlternativeIdentifier);
				writer.WriteAttributeString("CertHash", CertHash);
				writer.WriteAttributeString("IdCardId", IdCardId);
				writer.WriteAttributeString("Issuer", Issuer);
				writer.WriteAttributeString("Password", Password);
				writer.WriteAttributeString("Username", Username);
				writer.WriteAttributeString("Version", Version);
				writer.WriteAttributeString("AuthenticationLevel", AuthenticationLevel.Level.ToString());

				writer.WriteStartElement("CreatedDate");
				new XmlSerializer(typeof(DateTime)).Serialize(writer, this.CreatedDate);
				writer.WriteEndElement();

				writer.WriteStartElement("ExpiryDate");
				new XmlSerializer(typeof(DateTime)).Serialize(writer, this.ExpiryDate);
				writer.WriteEndElement();

				if (Xassertion != null)
				{
					writer.WriteStartElement("Xassertion");
					Xassertion.WriteTo(writer);
					writer.WriteEndElement();
				}

				SystemInfo.WriteXml(writer);
			}
			writer.WriteEndElement();
		}
	}
}
