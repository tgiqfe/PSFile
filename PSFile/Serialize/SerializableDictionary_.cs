/*
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PSFile
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public class KeyValue
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public KeyValue() { }
            public KeyValue(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KeyValue));

            reader.Read();
            if (reader.IsEmptyElement) { return; }
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                KeyValue kv = serializer.Deserialize(reader) as KeyValue;
                if (kv != null)
                {
                    Add(kv.Key, kv.Value);
                }
            }
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KeyValue));
            foreach (var key in Keys)
            {
                serializer.Serialize(writer, new KeyValue(key, this[key]),
                    new XmlSerializerNamespaces(
                        new XmlQualifiedName[1] { XmlQualifiedName.Empty }));
            }
        }
    }
}
*/