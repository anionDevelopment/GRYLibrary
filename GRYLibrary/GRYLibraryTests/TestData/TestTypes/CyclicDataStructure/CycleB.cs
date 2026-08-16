using GRYLibrary.Core.AOA;
using GRYLibrary.Core.AOA.SerializeHelper;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace GRYLibrary.Tests.TestData.TestTypes.CyclicDataStructure
{
    [Serializable]
    public class CycleB : IGRYSerializable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public CycleC C { get; set; }

        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
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

        public ISet<Type> GetExtraTypesWhichAreRequiredForSerialization()
        {
            return new HashSet<Type>();
        }
        #endregion
    }
}