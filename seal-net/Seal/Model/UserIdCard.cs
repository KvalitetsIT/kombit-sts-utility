using System;
using System.Xml.Linq;
using dk.nsi.seal.dgwstypes;
using Attribute = dk.nsi.seal.dgwstypes.Attribute;
using System.Xml.Serialization;
using System.Xml;

namespace dk.nsi.seal
{
	public class UserIdCard : SystemIdCard
    {
		[XmlElement]
		public UserInfo UserInfo { get; private set; }

		/// <summary>
		/// Empty constructor ONLY for Serialization/Deserialization
		/// </summary>
		public UserIdCard() { }

		public UserIdCard(IdCard toCopy, string issuer, string certHash, string alternativeIdentifier, SystemInfo systemInfo, UserInfo userInfo, AuthenticationLevel authLevel) : base(toCopy, issuer, certHash, alternativeIdentifier, systemInfo, authLevel)
        {
            UserInfo = userInfo;
        }

        public UserIdCard(string version, AuthenticationLevel authLevel, string issuer, SystemInfo systemInfo, UserInfo userInfo, string certHash, string alternativeIdentifier, string userName, string password) : base(version, authLevel, issuer, systemInfo, certHash, alternativeIdentifier, userName, password)
        {
            ModelUtilities.ValidateNotNull(userInfo, "UserInfo must be specified");
            UserInfo = userInfo;
        }

        public UserIdCard(string version, XElement xAssertion, string cardId, AuthenticationLevel authLevel, string certHash, string issuer, SystemInfo systemInfo, UserInfo userInfo, DateTime creationDate, DateTime expiryDate, string alternativeIdentifier, string username, string password) : base(version, xAssertion, cardId, authLevel, certHash, issuer, systemInfo, creationDate, expiryDate, alternativeIdentifier, username, password)
        {
            ModelUtilities.ValidateNotNull(userInfo, "UserInfo must be specified");
            this.UserInfo = userInfo;
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
            if (string.IsNullOrEmpty(AlternativeIdentifier))
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
                            new Attribute {Name = AttributeName.sosiIDCardType, AttributeValue = "user"},
                            new Attribute {Name = AttributeName.sosiAuthenticationLevel, AttributeValue = AuthenticationLevel.Level.ToString()},
                            new Attribute {Name = AttributeName.sosiOCESCertHash, AttributeValue = CertHash,}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.UserLog,
                        Attribute = new[]
                        {
                            new Attribute {Name = AttributeName.medcomUserCivilRegistrationNumber, AttributeValue = UserInfo.Cpr},
                            new Attribute {Name = AttributeName.medcomUserGivenName, AttributeValue = UserInfo.GivenName},
                            new Attribute {Name = AttributeName.medcomUserSurName, AttributeValue = UserInfo.SurName},
							string.IsNullOrEmpty(UserInfo.Email) ? null : new Attribute {Name = AttributeName.medcomUserEmailAddress, AttributeValue = UserInfo.Email},

							new Attribute {Name = AttributeName.medcomUserRole, AttributeValue = UserInfo.Role},
							string.IsNullOrEmpty(UserInfo.AuthorizationCode) ? null : new Attribute {Name = AttributeName.medcomUserAuthorizationCode, AttributeValue = UserInfo.AuthorizationCode},
							string.IsNullOrEmpty(UserInfo.Occupation) ? null : new Attribute {Name = AttributeName.medcomUserOccupation, AttributeValue = UserInfo.Occupation}
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
		    var otherUserIdCard = obj as UserIdCard;
		    if (otherUserIdCard == null)
			    return false;
		    var result = base.Equals(obj) &&
		                  UserInfo.Equals(otherUserIdCard.UserInfo);
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
						if (reader.Name == "UserInfo")
						{
							this.UserInfo = new UserInfo(null, null, null, null, null, null, null);
							(this.UserInfo as IXmlSerializable).ReadXml(reader);
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

				if(Xassertion != null)
				{
					writer.WriteStartElement("Xassertion");
					Xassertion.WriteTo(writer);
					writer.WriteEndElement();
				}

				SystemInfo.WriteXml(writer);

				UserInfo.WriteXml(writer);
			}
			writer.WriteEndElement();
		}
	}
}
