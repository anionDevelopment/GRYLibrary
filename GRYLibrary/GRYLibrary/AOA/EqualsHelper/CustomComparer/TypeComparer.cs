using System;

namespace GRYLibrary.Core.AOA.EqualsHelper.CustomComparer
{
    public class TypeComparer : AbstractCustomComparer
    {

        public TypeComparer(PropertyEqualsCalculatorConfiguration cacheAndConfiguration) : base(cacheAndConfiguration)
        {
            this.Configuration = cacheAndConfiguration;
        }

        public override bool DefaultEquals(object item1, object item2)
        {
            bool result = this.EqualsTyped((Type)item1, (Type)item2);
            return result;
        }

        internal bool EqualsTyped(Type type1, Type type2)
        {
            return type1.Equals(type2);
        }

        public override int DefaultGetHashCode(object obj)
        {
            return this.Configuration.GetHashCode(obj);
        }

        public override bool IsApplicable(Type typeOfObject1, Type typeOfObject2)
        {
            return Misc.Utilities.TypeIsAssignableFrom(typeOfObject1, typeof(Type)) && Misc.Utilities.TypeIsAssignableFrom(typeOfObject2, typeof(Type));
        }
    }
}