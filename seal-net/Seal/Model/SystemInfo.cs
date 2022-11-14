using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace dk.nsi.seal
{

    public class SystemInfo : IXmlSerializable
	{
        private CareProvider careProvider;

        public CareProvider CareProvider { get; private set; }

		public string ItSystemName { get; private set; }

		private SystemInfo()
		{

		}
		public SystemInfo(CareProvider careProvider, string itSystemName)
		{
			CareProvider = careProvider;
			ItSystemName = itSystemName;
		}

        public SystemInfo(CareProvider careProvider)
        {
            this.careProvider = careProvider;
        }

        public override bool Equals(object obj)
		{
			if (!(obj is SystemInfo)) return false;

			var si = (SystemInfo)obj;
			var result = CareProvider.Id.Equals(si.CareProvider.Id)
			              & CareProvider.OrgName.Equals(si.CareProvider.OrgName)
			              & CareProvider.Type.Equals(si.CareProvider.Type)
			              & ItSystemName.Equals(si.ItSystemName);
			return result;
		}

		public override int GetHashCode()
		{
			return CareProvider.GetHashCode()
					^ ItSystemName.GetHashCode();
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			ItSystemName = reader.GetAttribute("ItSystemName");

			reader.ReadStartElement();
			if (reader.Name == "CareProvider")
			{
				this.CareProvider = new CareProvider(dgwstypes.SubjectIdentifierType.medcomcommunalnumber, null, null);
				(this.CareProvider as IXmlSerializable).ReadXml(reader);
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("SystemInfo");
			{
				writer.WriteAttributeString("ItSystemName", ItSystemName);
				CareProvider.WriteXml(writer);

			}
			writer.WriteEndElement();
		}
	}
}
