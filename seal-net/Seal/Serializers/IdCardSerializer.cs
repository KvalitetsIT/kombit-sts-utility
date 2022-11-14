using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace dk.nsi.seal.Serializers
{
	public static class IdCardSerializer
	{

		/// <summary>
		/// Serializes an IdCard to a Xml document in string form
		/// </summary>
		/// <typeparam name="T">IdCard type (either SystemIdCard or UserIdCard)</typeparam>
		/// <param name="idCard">IdCard to serialize</param>
		/// <returns>Xml representation of IdCard as string</returns>
		public static string SerializeIdCardToString<T>(T idCard)
		{
			var reader = new StreamReader(Serialize2Stream<T>(idCard));
			return reader.ReadToEnd();
		}

		/// <summary>
		/// Serializes an IdCard to a Xml document in a MemoryStream
		/// </summary>
		/// <typeparam name="T">IdCard type (either SystemIdCard or UserIdCard)</typeparam>
		/// <param name="idCard">IdCard to serialize</param>
		/// <returns>Xml representation of IdCard as MemoryStream</returns>
		public static MemoryStream SerializeIdCardToStream<T>(T idCard)
		{
			return Serialize2Stream<T>(idCard);
		}

		/// <summary>
		/// Deserialize IdCard from string
		/// </summary>
		/// <typeparam name="T">IdCard type (either SystemIdCard or UserIdCard)</typeparam>
		/// <param name="serializedIdCard">String of serialized IdCard</param>
		/// <returns>IdCard from string</returns>
		public static T DeserializeIdCard<T>(string serializedIdCard) where T : class
		{
			var xDoc = XDocument.Parse(serializedIdCard, LoadOptions.PreserveWhitespace);
			return Deserialize<T>(xDoc.Root);
		}

		/// <summary>
		/// Deserialize IdCard from Stream
		/// </summary>
		/// <typeparam name="T">IdCard type (either SystemIdCard or UserIdCard)</typeparam>
		/// <param name="serializedIdCard">Stream of serialized IdCard</param>
		/// <returns>IdCard from Stream</returns>
		public static T DeserializeIdCard<T>(Stream serializedIdCard) where T : class
		{
			var xDoc = XDocument.Load(serializedIdCard, LoadOptions.PreserveWhitespace);
			return Deserialize<T>(xDoc.Root);
		}

		private static MemoryStream Serialize2Stream<T>(T element)
		{
			var ms = new MemoryStream();
			using (var xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings { Indent = false }))
			{
				var xmlSerializer = new XmlSerializer(typeof(T));
				xmlSerializer.Serialize(xmlWriter, element);
			}
			ms.Position = 0;
			return ms;
		}

		private static T Deserialize<T>(XElement document) where T : class
		{
			var ms = new MemoryStream();
			document.Save(ms);
			ms.Position = 0;
			var xmlSerializer = new XmlSerializer(typeof(T));
			return xmlSerializer.Deserialize(ms) as T;
		}
	}
}
