using GRYLibrary.Core.AOA;
using GRYLibrary.Tests.TestData.TestTypes.CyclicDataStructure;
using GRYLibrary.Tests.TestData.TestTypes.GenericType;
using GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GRYLibrary.Tests.TestData.TestTypes.XMLSerializableType
{
    public class XMLSerializableType : IXmlSerializable
    {
        public XMLSerializableType() { }
        public CycleA Cyle { get; set; }
        public SimpleDataStructure1 SimpleDataStructure1 { get; set; }
        public GenericType<int> GenericInt { get; set; }
        public GenericType<SimpleDataStructure1> GenericSimpleDataStructure1 { get; set; }
        internal static XMLSerializableType GetRandom()
        {
            XMLSerializableType result = new()
            {
                Cyle = CycleA.GetRandom(),
                SimpleDataStructure1 = SimpleDataStructure1.GetRandom(),
                GenericInt = new GenericType<int>()
                {
                    TList = [21, 22],
                    TSet = [23, 24],
                    TObject = 42,
                    Enumerable = new ArrayList() { 25, 26 },
                    TDictionary1 = new Dictionary<string, int>() { { "key1", 27 }, { "key2", 28 } },
                    TDictionary2 = new Dictionary<int, int>() { { 29, 30 }, { 31, 32 } },
                    TEnumerable = new List<int>() { 27, 28 },
                },
                GenericSimpleDataStructure1 = new GenericType<SimpleDataStructure1>()
                {
                    TList = [SimpleDataStructure1.GetRandom(), SimpleDataStructure1.GetRandom()],
                    TSet = [SimpleDataStructure1.GetRandom(), SimpleDataStructure1.GetRandom()],
                    TDictionary1 = new Dictionary<string, SimpleDataStructure1>() { { "key", null } },
                    TDictionary2 = new Dictionary<int, SimpleDataStructure1>() { { 5, SimpleDataStructure1.GetRandom() }, { default, SimpleDataStructure1.GetRandom() } },
                    TObject = SimpleDataStructure1.GetRandom()
                },
            };
            result.GenericInt.TKeyValuePair1 = new KeyValuePair<int, string>(27, "key1");
            result.GenericSimpleDataStructure1.TKeyValuePair1 = new KeyValuePair<SimpleDataStructure1, string>(SimpleDataStructure1.GetRandom(), "key1");
            return result;
        }

        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
        }

        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }

        public XmlSchema GetSchema()
        {
            return Generic.GenericGetSchema(this);
        }

        public void ReadXml(XmlReader reader)
        {
            Generic.GenericReadXml(this, reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            Generic.GenericWriteXml(this, writer);
        }
        #endregion
    }
}