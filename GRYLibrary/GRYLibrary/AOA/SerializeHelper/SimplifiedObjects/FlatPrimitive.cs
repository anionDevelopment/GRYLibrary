using System.Xml.Serialization;

namespace GRYLibrary.Core.AOA.SerializeHelper.SimplifiedObjects
{
    /// <summary>
    /// Represents a SimplifiedPrimitive for <see cref="GRYSObject"/>
    /// </summary>
    [XmlRoot(ElementName = "FP")]
    public class FlatPrimitive : FlatObject
    {
        public FlatPrimitive()
        {
        }
        public object Value { get; set; }
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