using System.Collections.Generic;
using System.Xml.Serialization;

namespace GRYLibrary.Core.XMLSerializer
{
    [XmlRoot("Dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        private readonly XmlSerializer _KeySerializer;
        private readonly XmlSerializer _ValueSerializer;
        public SerializableDictionary()
        {
            this._KeySerializer = new XmlSerializer(typeof(TKey));
            this._ValueSerializer = new XmlSerializer(typeof(TValue));
        }
        public SerializableDictionary(IDictionary<TKey, TValue> values) : this()
        {
            foreach (System.Collections.Generic.KeyValuePair<TKey, TValue> kvp in values)
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool isEmptyElement = reader.IsEmptyElement;
            reader.Read();
            if (isEmptyElement)
            {
                return;
            }
            else
            {
                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("Item");
                    reader.ReadStartElement("Key");
                    TKey key = (TKey)this._KeySerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    reader.ReadStartElement("Value");
                    TValue value = (TValue)this._ValueSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    this.Add(key, value);
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                this._KeySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                this._ValueSerializer.Serialize(writer, this[key]);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
    }
}