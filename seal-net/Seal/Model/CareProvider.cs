using dk.nsi.seal.dgwstypes;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace dk.nsi.seal
{

    public class CareProvider : IXmlSerializable
	{


		public string Id { get; private set; }

		public string OrgName { get; private set; }

		public SubjectIdentifierType Type { get; private set; }

		private CareProvider()
		{

		}

		public CareProvider(SubjectIdentifierType type, string id, string orgName)
		{
			Id = id;
			OrgName = orgName;
			Type = type;

		}

		public override bool Equals(object obj)
		{
			if (!(obj is CareProvider)) return false;

			var cp = (CareProvider)obj;
			return Id == cp.Id
				& OrgName == cp.OrgName
				& Type == cp.Type;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode()
					^ OrgName.GetHashCode()
					^ Type.GetHashCode();
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			Id = reader.GetAttribute("Id");
			OrgName = reader.GetAttribute("OrgName");
			reader.ReadStartElement();
			if (reader.Name == "SubjectIdentifierType")
			{
				reader.ReadStartElement();
				this.Type = (SubjectIdentifierType)new XmlSerializer(typeof(SubjectIdentifierType)).Deserialize(reader);
				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("CareProvider");
			{
				writer.WriteAttributeString("Id", Id);
				writer.WriteAttributeString("OrgName", OrgName);
				writer.WriteStartElement("SubjectIdentifierType");
				new XmlSerializer(typeof(SubjectIdentifierType)).Serialize(writer, this.Type);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
	}
}
