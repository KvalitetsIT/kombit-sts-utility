using System;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal.Constants;
using dk.nsi.seal.dgwstypes;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class IdCardModelBuilder
    {

		/// <summary>
		/// Builds an ID-card objects from a DOM document.
		/// </summary>
		/// <param name="doc">The DOM document used for the ID-card.</param>
		/// <returns>A new <see cref="IdCard"/></returns>
		public IdCard BuildModel(XDocument doc)
        {
            IdCard result = null;

            var assertions = doc.Elements(NameSpaces.saml + SamlTags.Assertion);
            foreach (var assertion in assertions)
            {
                foreach (var xAttribute in assertion.Attributes())
                {
                    if (xAttribute.Name.LocalName.ToLower().EndsWith("id") && xAttribute.Value.Equals(IdValues.IdCard))
                    {
                        result = InternalBuild(assertion);
                        break;
                    }
                }
            }
            return result;
        }


		/// <summary>
		/// Builds an ID-card object from a DOM element.
		/// </summary>
		/// <param name="assertion">The DOM element for the ID-card</param>
		/// <returns>A new <see cref="IdCard"/></returns>
		public IdCard BuildModel(XElement assertion)
        {
            return InternalBuild(assertion);
        }


        private IdCard InternalBuild(XElement idCardElement)
        {
            IdCard result;
            string itSystemName = null,
                ocesCertHash = null,
                id = null,
                version = null,
                cpr = null,
                givenName = null,
                surName = null,
                email = null,
                occupation = null,
                userRole = null,
                authorizationCode = null,
                careProviderId = null,
                careProviderIdType = null,
                careProviderName = null,
                authLevel = null;
            bool hasIdCardData = false, hasSystemLog = false, hasUserLog = false;

            string alternativeIdentifier = null;
            string username = null;
            string password = null;

            DateTime createdDate = new DateTime(), expiryDate = new DateTime();

            // Check validity interval
            var timeConstraints =
                idCardElement.Descendants("{" + SamlTags.Conditions.Ns + "}" + SamlTags.Conditions.TagName);
            var conditionsAttributes = timeConstraints.Attributes();

            try
            {
                foreach (var attribute in conditionsAttributes)
                {
                    var attributeValue = attribute.Value;
                    var attributeName = attribute.Name;
                    if (SamlAttributes.NotOnOrAfter == attributeName)
                    {
                        expiryDate = DateTime.Parse(attributeValue);
                    }
                    else if (SamlAttributes.NotBefore == attributeName)
                    {
                        createdDate = DateTime.Parse(attributeValue);
                    }
                }
            }
            catch (Exception e)
            {
                throw new ModelBuildException("SAML:Conditions could not be parsed", e);
            }

            //Check for an alternative Identifier
            var subjectNameIdNode =
                idCardElement.Descendants("{" + SamlTags.NameID.Ns + "}" + SamlTags.NameID.TagName).FirstOrDefault();
            var nameIdFormatNode = subjectNameIdNode.Attribute((SamlAttributes.Format));
            if (nameIdFormatNode.Value.Equals(SubjectIdentifierTypeValues.Other))
            {
                alternativeIdentifier = subjectNameIdNode.Value;
            }

            // IDCard attributes
            var issuerNode = idCardElement.Descendants("{" + SamlTags.Issuer.Ns + "}" + SamlTags.Issuer.TagName).FirstOrDefault();
            var issuer = issuerNode.Value;

            var attributeStatementNodeList = idCardElement.Descendants("{" + SamlTags.AttributeStatement.Ns + "}" + SamlTags.AttributeStatement.TagName);

            bool? isUserIDCard = null;
            foreach (var attributeStatement in attributeStatementNodeList)
            {
                var map = attributeStatement.Attributes();
                foreach (var attribute in map)
                {
                    var attributeValue = attribute.Value;

                    if (IdValues.SystemLog.Equals(attributeValue))
                    {
                        // Iterate saml:Attributes in SystemLog
                        var samlAttributeNodes =
                            attributeStatement.Descendants("{" + SamlTags.Attribute.Ns + "}" + SamlTags.Attribute.TagName);

                        foreach (var samlAttribute in samlAttributeNodes)
                        {
                            var attributeName = samlAttribute.Attribute("Name").Value;
                            var attributeNameValue = GetAttributeNameValue(samlAttribute, attributeName);
                            if (MedComAttributes.ItSystemName.Equals(attributeName))
                            {
                                itSystemName = attributeNameValue;
                            }
                            else if (MedComAttributes.CareProviderId.Equals(attributeName))
                            {
                                careProviderId = attributeNameValue;
                                var nameFormatAttribute = samlAttribute.Attribute(SamlAttributes.NameFormat);
                                if (nameFormatAttribute == null)
                                {
                                    throw new ModelBuildException(
                                        "DGWS violation: 'medcom:CareProviderID' SAML attribute must contain a 'NameFormat' attribute!");
                                }
                                careProviderIdType = nameFormatAttribute.Value;
                            }
                            else if (MedComAttributes.CareProviderName.Equals(attributeName))
                            {
                                careProviderName = attributeNameValue;
                            }
                        }
                        hasSystemLog = true;
                    }
                    else if (IdValues.IdCardData.Equals(attributeValue))
                    {
                        // Iterate saml:Attributes in IDCard
                        var samlAttributeNodes =
                            attributeStatement.Descendants("{" + SamlTags.Attribute.Ns + "}" + SamlTags.Attribute.TagName);

                        foreach (var samlAttribute in samlAttributeNodes)
                        {
                            var attributeName = samlAttribute.Attribute("Name").Value;
                            var attributeNameValue = GetAttributeNameValue(samlAttribute, attributeName);
                            // Cert Hash
                            if (SosiAttributes.OcesCertHash.Equals(attributeName))
                            {
                                ocesCertHash = attributeNameValue;
                                // CardID
                            }
                            else if (SosiAttributes.IDCardID.Equals(attributeName))
                            {
                                id = attributeNameValue;
                                // CardVersion
                            }
                            else if (SosiAttributes.IDCardVersion.Equals(attributeName))
                            {
                                version = attributeNameValue;
                                // IDCardType
                            }
                            else if (SosiAttributes.IDCardType.Equals(attributeName))
                            {
                                if (IdCard.IDCARDTYPE_USER.Equals(attributeNameValue))
                                    isUserIDCard = true;
                                else if (IdCard.IDCARDTYPE_SYSTEM.Equals(attributeNameValue))
                                    isUserIDCard = false;
                            }
                            else if (SosiAttributes.AuthenticationLevel.Equals(attributeName))
                            {
                                authLevel = attributeNameValue;
                            }
                        }
                        hasIdCardData = true;
                    }
                    else if (IdValues.UserLog.Equals(attributeValue))
                    {
                        // Iterate saml:Attributes in UserLog
                        var samlAttributeNodes =
                            attributeStatement.Descendants("{" + SamlTags.Attribute.Ns + "}" + SamlTags.Attribute.TagName);

                        foreach (var samlAttribute in samlAttributeNodes)
                        {
                            var attributeName = samlAttribute.Attribute("Name").Value;
                            var attributeNameValue = GetAttributeNameValue(samlAttribute, attributeName);
                            if (MedComAttributes.UserCivilRegistrationNumber.Equals(attributeName))
                            {
                                cpr = attributeNameValue;
                            }
                            else if (MedComAttributes.UserGivenName.Equals(attributeName))
                            {
                                givenName = attributeNameValue;
                            }
                            else if (MedComAttributes.UserSurname.Equals(attributeName))
                            {
                                surName = attributeNameValue;
                            }
                            else if (MedComAttributes.UserEmailAddress.Equals(attributeName))
                            {
                                email = attributeNameValue;
                            }
                            else if (MedComAttributes.UserOccupation.Equals(attributeName))
                            {
                                occupation = attributeNameValue;
                            }
                            else if (MedComAttributes.UserRole.Equals(attributeName))
                            {
                                userRole = attributeNameValue;
                            }
                            else if (MedComAttributes.UserAuthorizationCode.Equals(attributeName))
                            {
                                authorizationCode = attributeNameValue;
                            }
                        }
                        hasUserLog = true;
                    }
                }
            }
            SubjectIdentifierType careProviderIdEnum;
            Enum.TryParse(careProviderIdType.Replace(":",""), true, out careProviderIdEnum);
            CareProvider careProvider = new CareProvider(careProviderIdEnum, careProviderId, careProviderName);
            SystemInfo systemInfo = new SystemInfo(careProvider, itSystemName);

            // All IDCard types must have a IDCardData element
            if (!hasIdCardData) throw new ModelBuildException("IDCardData element missing for IDCard");

            // All IDCard types must have a SystemLog element
            if (!hasSystemLog) throw new ModelBuildException("SystemLog element missing for IDCard");

			if (isUserIDCard == null)
				throw new ModelBuildException("ID Card type not found or invalid");
			else if (isUserIDCard.Value)
			{
				if (!hasUserLog) throw new ModelBuildException("UserLog element missing for UserIDCard");
				UserInfo userInfo = new UserInfo(cpr, givenName, surName, email, occupation, userRole, authorizationCode);
				result = new UserIdCard(version, idCardElement, id,
					AuthenticationLevel.GetEnumeratedValue(int.Parse(authLevel)),
                    ocesCertHash, issuer, systemInfo, userInfo, createdDate, expiryDate, alternativeIdentifier, username, password);
			}
			else
			{
				if (hasUserLog) throw new ModelBuildException("IDCard type is 'system', but also has a UserLog element (??)");
				result = new SystemIdCard(version, idCardElement, id,
					AuthenticationLevel.GetEnumeratedValue(int.Parse(authLevel)),
                     ocesCertHash, issuer, systemInfo, createdDate, expiryDate, alternativeIdentifier, username, password);
			}
			return result;
		}

		private string GetAttributeNameValue(XElement samlAttribute, string attributeName)
		{
			var elmAttributeValue = samlAttribute.DescendantsAndSelf("{" + SamlTags.AttributeValue.Ns + "}" + SamlTags.AttributeValue.TagName).FirstOrDefault();
			if (elmAttributeValue == null)
				throw new ModelBuildException("Missing 'saml:AttributeValue' element for 'saml:Attribute' element '" + attributeName +
											  "'");
			return elmAttributeValue.Value;
		}
	}
}
