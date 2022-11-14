using System.Xml.Linq;

namespace dk.nsi.seal.Model.Constants
{
    public class SamlTags : ITag
	{
		public static SamlTags Assertion => new SamlTags("Assertion");
		public static SamlTags EncryptedAssertion => new SamlTags("EncryptedAssertion");
		public static SamlTags Conditions => new SamlTags("Conditions");
		public static SamlTags AudienceRestriction => new SamlTags("AudienceRestriction");
		public static SamlTags AttributeStatement => new SamlTags("AttributeStatement");
		public static SamlTags Attribute => new SamlTags("Attribute");
		public static SamlTags AttributeValue => new SamlTags("AttributeValue");
		public static SamlTags AuthnStatement => new SamlTags("AuthnStatement");
		public static SamlTags AuthnContext => new SamlTags("AuthnContext");
		public static SamlTags AuthnContextClassRef => new SamlTags("AuthnContextClassRef");
		public static SamlTags Audience => new SamlTags("Audience");
		public static SamlTags Subject => new SamlTags("Subject");
		public static SamlTags Issuer => new SamlTags("Issuer");
		public static SamlTags NameID => new SamlTags("NameID");
		public static SamlTags SubjectConfirmationData => new SamlTags("SubjectConfirmationData");
		public static SamlTags SubjectConfirmation => new SamlTags("SubjectConfirmation");

		protected SamlTags(string tag)
		{
			TagName = tag;
		}
		public XNamespace Ns => NameSpaces.xsaml;
		public string TagName { get; private set; }
	}

}
