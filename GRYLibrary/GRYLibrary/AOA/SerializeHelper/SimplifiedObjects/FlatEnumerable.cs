using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GRYLibrary.Core.AOA.SerializeHelper.SimplifiedObjects
{
    /// <summary>
    /// Represents a SimplifiedEnumerable for <see cref="GRYSObject"/>
    /// </summary>
    [XmlRoot(ElementName = "FE")]
    public class FlatEnumerable : FlatObject
    {
        public List<Guid> Items { get; set; }

        public override void Accept(IFlatObjectVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IFlatObjectVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}