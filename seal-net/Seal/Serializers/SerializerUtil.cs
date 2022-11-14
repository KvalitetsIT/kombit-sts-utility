using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace dk.nsi.seal
{
    public class SerializerUtil
    {
        private static readonly Dictionary<string, XmlSerializer> Serializers = new Dictionary<string, XmlSerializer>();

        private static XmlSerializer GetSerializer<T>()
        {
            var t = typeof(T);
            var fn = t.FullName;
            if (Serializers.ContainsKey(fn)) return Serializers[fn];

            var rootns = t.GetCustomAttributes(false).OfType<XmlTypeAttribute>().FirstOrDefault()?.Namespace;
            if (rootns == null) throw new InvalidOperationException("Unknown root namespace");
            Serializers.Add(fn, new XmlSerializer(t, rootns));
            return Serializers[fn];
        }

        private static XmlSerializer GetSerializer<T>(string rootName)
        {
            var t = typeof(T);
            var fn = t.FullName + rootName;
            if (Serializers.ContainsKey(fn)) return Serializers[fn];

            var rootns = t.GetCustomAttributes(false).OfType<XmlTypeAttribute>().FirstOrDefault().Namespace;
            Serializers.Add(fn, new XmlSerializer(t, new XmlRootAttribute(rootName) { Namespace = rootns }));
            return Serializers[fn];
        }

        private static Stream Serialize2Stream<T>(T element)
        {
            var ms = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings { Indent = false }))
            {
                GetSerializer<T>().Serialize(xmlWriter, element);
            }
            ms.Position = 0;
            return ms;
        }

        public static XDocument Serialize<T>(T element)
        {
            return XDocument.Load(Serialize2Stream(element), LoadOptions.PreserveWhitespace);
        }

        public static T Deserialize<T>(XElement document) where T : class
        {
            var ms = new MemoryStream();
            document.Save(ms);
            ms.Position = 0;
            return GetSerializer<T>().Deserialize(ms) as T;
        }

        public static T Deserialize<T>(XElement document, string rootId) where T : class
        {
            var ms = new MemoryStream();
            document.Save(ms);
            ms.Position = 0;
            return GetSerializer<T>(rootId).Deserialize(ms) as T;
        }

        internal static T Deserialize<T>(Stream stream) where T : class
        {
            return GetSerializer<T>().Deserialize(stream) as T;
        }

        internal static T Deserialize<T>(XmlElement elm) where T : class
        {
            using (var nr = new XmlNodeReader(elm))
            {
                return GetSerializer<T>().Deserialize(nr) as T;
            }
        }
    }
}