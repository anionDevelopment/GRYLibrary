using GRYLibrary.Core.AOA;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GRYLibrary.Tests.TestData.TestTypes.AdvancedObjectAnalysisUsageFull
{
    public class TestType : IXmlSerializable
    {
        public TestType()
        {
        }
        public TestAttributeType AttributeA { get; set; }
        public TestAttributeType AttributeB { get; set; }
        public static TestType GetRandom()
        {
            TestType result = new();
            result.AttributeA = new TestAttributeType
            {
                ComplexAttribute = result.AttributeB,
                IntAttribute = 2
            };
            result.AttributeB = new TestAttributeType
            {
                ComplexAttribute = result.AttributeA,
                IntAttribute = 3
            };
            return result;
        }
        #region Overhead
        public override bool Equals(object obj)
        {
            return Generic.GenericEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }
        public override string ToString()
        {
            return Generic.GenericToString(this);
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