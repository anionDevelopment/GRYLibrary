using GRYLibrary.Core.AOA.SerializeHelper.SimplifiedObjects;
using System;
using System.Xml.Serialization;

namespace GRYLibrary.Core.AOA.SerializeHelper
{
    [XmlInclude(typeof(FlatEnumerable))]
    [XmlInclude(typeof(FlatComplexObject))]
    [XmlInclude(typeof(FlatPrimitive))]
    public abstract class FlatObject
    {
        public Guid ObjectId { get; set; }
        public string TypeName { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is FlatComplexObject typedObject)
            {
                return this.ObjectId.Equals(typedObject.ObjectId);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ObjectId);
        }

        public abstract void Accept(IFlatObjectVisitor visitor);
        public abstract T Accept<T>(IFlatObjectVisitor<T> visitor);
        public interface IFlatObjectVisitor
        {
            void Handle(FlatComplexObject simplifiedObject);
            void Handle(FlatEnumerable simplifiedEnumerable);
            void Handle(FlatPrimitive simplifiedPrimitive);
        }
        public interface IFlatObjectVisitor<T>
        {
            T Handle(FlatComplexObject simplifiedObject);
            T Handle(FlatEnumerable simplifiedEnumerable);
            T Handle(FlatPrimitive simplifiedPrimitive);
        }
    }
}