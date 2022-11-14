using System.Collections.Generic;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;
using dk.nsi.seal.Model.ModelBuilders;

namespace dk.nsi.seal.Model.Requests
{
    public class IdCardToOioSamlAssertionRequest : OioWsTrustRequest
	{

		public UserIdCard UserIdCard
		{
			get
			{
				var assertion = GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Body, WstTags.RequestSecurityToken, Wst14Tags.ActAs, SamlTags.Assertion });
				if (assertion == null)
				{
					assertion = GetTag(new List<ITag>() { SoapTags.Envelope, SoapTags.Header, WsseTags.Security, SamlTags.Assertion });
				}

				if (assertion == null)
				{
					throw new ModelException("Malformed request: IDCard could not be found!");
				}

				var idCard = new IdCardModelBuilder().BuildModel(assertion) as UserIdCard;
				if (idCard == null)
				{
					throw new ModelException("IDCard in request is not a UserIDCard!");
				}
				return idCard;

			}
		}

		public IdCardToOioSamlAssertionRequest(XDocument doc) : base(doc)
		{

		}
	}
}
