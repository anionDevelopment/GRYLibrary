using GRYLibrary.Core.Misc;
using System;
using System.Collections;

namespace GRYLibrary.Core.AOA.EqualsHelper.CustomComparer
{
    public class EnumerableComparer : AbstractCustomComparer
    {
        internal EnumerableComparer(PropertyEqualsCalculatorConfiguration cacheAndConfiguration) : base(cacheAndConfiguration)
        {
            this.Configuration = cacheAndConfiguration;
        }

        public override bool DefaultEquals(object item1, object item2)
        {
            bool result = this.EqualsTyped(EnumerableTools.ObjectToEnumerable(item1), EnumerableTools.ObjectToEnumerable(item2));
            return result;
        }
        internal bool EqualsTyped(IEnumerable enumerable1, IEnumerable enumerable2)
        {
            if (enumerable1.Count() != enumerable2.Count())
            {
                return false;
            }
            foreach (object item in enumerable1)
            {
                if (this.GetCountOfItemInEnumerable(enumerable1, item) != this.GetCountOfItemInEnumerable(enumerable2, item))
                {
                    return false;
                }
            }
            return true;
        }

        private int GetCountOfItemInEnumerable(IEnumerable enumerable, object item)
        {
            int result = 0;
            foreach (object enumerableEntry in enumerable)
            {
                if (this._PropertyEqualsCalculator.Equals(item))
                {
                    result += 1;
                }
            }

            return result;
        }
        public override int DefaultGetHashCode(object obj)
        {
            return this.Configuration.GetHashCode(obj);
        }

        public override bool IsApplicable(Type typeOfObject1, Type typeOfObject2)
        {
            return EnumerableTools.TypeIsEnumerable(typeOfObject1) && EnumerableTools.TypeIsEnumerable(typeOfObject2);
        }
    }
}