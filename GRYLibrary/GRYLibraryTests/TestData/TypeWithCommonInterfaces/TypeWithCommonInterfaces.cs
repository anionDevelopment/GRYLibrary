using GRYLibrary.Core.AOA;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GRYLibrary.Tests.TestData.TypeWithCommonInterfaces
{
    public class TypeWithCommonInterfaces : IXmlSerializable
    {
        public IList<object> List { get; set; }
        public IList<object> List2 { get; set; } = null;
        public IEnumerable<int> Enumerable { get; set; }
        public ISet<int> Set { get; set; }
        //public string[] Array { get; set; }
        public IDictionary<int, int> Dictionary { get; set; }

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