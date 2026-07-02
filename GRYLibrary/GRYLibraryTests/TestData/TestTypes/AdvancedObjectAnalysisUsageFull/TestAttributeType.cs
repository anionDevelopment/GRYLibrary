using GRYLibrary.Core.AOA;
using System.Xml;
using System.Xml.Schema;

namespace GRYLibrary.Tests.TestData.TestTypes.AdvancedObjectAnalysisUsageFull
{
    public class TestAttributeType
    {
        public TestAttributeType ComplexAttribute { get; set; }
        public int IntAttribute { get; set; }
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