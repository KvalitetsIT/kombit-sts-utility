using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace dk.nsi.seal
{
    public class UserInfo : IXmlSerializable
    {
        public string AuthorizationCode { get; private set; }
        public string Cpr { get; private set; }
        public string Email { get; private set; }
        public string GivenName { get; private set; }
        public string Occupation { get; private set; }
        public string Role { get; private set; }
        public string SurName { get; private set; }

        private UserInfo()
        {

        }

        public UserInfo(string cpr, string givenName, string surName, string email, string occupation, string role, string authorizationCode)
        {
            Cpr = cpr;
            GivenName = givenName;
            SurName = surName;
            Email = email;
            Occupation = occupation;
            Role = role;
            AuthorizationCode = authorizationCode;
        }
        public UserInfo(UserInfo original, string cpr)
        {
            Cpr = cpr;
            GivenName = original.GivenName;
            SurName = original.SurName;
            Email = original.Email;
            Occupation = original.Occupation;
            Role = original.Role;
            AuthorizationCode = original.AuthorizationCode;

        }


        public override bool Equals(object obj)
        {
            if (!(obj is UserInfo)) return false;

            var ui = (UserInfo)obj;
            var result = Cpr == ui.Cpr
                          & GivenName == ui.GivenName
                          & SurName == ui.SurName
                          & Email == ui.Email
                          & Occupation == ui.Occupation
                          & Role == ui.Role
                          & AuthorizationCode == ui.AuthorizationCode;
            return result;
        }

        public override int GetHashCode()
        {
            return Cpr.GetHashCode()
                    ^ GivenName.GetHashCode()
                    ^ SurName.GetHashCode()
                    ^ Email.GetHashCode()
                    ^ Occupation.GetHashCode()
                    ^ Role.GetHashCode()
                    ^ AuthorizationCode.GetHashCode();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            AuthorizationCode = reader.GetAttribute("AuthorizationCode");
            Cpr = reader.GetAttribute("Cpr");
            Email = reader.GetAttribute("Email");
            GivenName = reader.GetAttribute("GivenName");
            Occupation = reader.GetAttribute("Occupation");
            Role = reader.GetAttribute("Role");
            SurName = reader.GetAttribute("SurName");
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("UserInfo");
            {
                writer.WriteAttributeString("AuthorizationCode", AuthorizationCode);
                writer.WriteAttributeString("Cpr", Cpr);
                writer.WriteAttributeString("Email", Email);
                writer.WriteAttributeString("GivenName", GivenName);
                writer.WriteAttributeString("Occupation", Occupation);
                writer.WriteAttributeString("Role", Role);
                writer.WriteAttributeString("SurName", SurName);
            }
            writer.WriteEndElement();
        }

    }
}
