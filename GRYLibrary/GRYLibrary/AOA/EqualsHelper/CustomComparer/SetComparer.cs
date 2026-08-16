using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.AOA.EqualsHelper.CustomComparer
{
    public class SetComparer : AbstractCustomComparer
    {
        internal SetComparer(PropertyEqualsCalculatorConfiguration cacheAndConfiguration) : base(cacheAndConfiguration)
        {
            this.Configuration = cacheAndConfiguration;
        }

        public override bool DefaultEquals(object item1, object item2)
        {
            bool result = this.EqualsTyped(EnumerableTools.ObjectToSet<object>(item1), EnumerableTools.ObjectToSet<object>(item2));
            return result;
        }
        internal bool EqualsTyped<T>(ISet<T> set1, ISet<T> set2)
        {
            if (set1.Count != set2.Count)
            {
                return false;
            }
            foreach (T obj in set1)
            {
                if (!this.Contains(set2, obj))
                {
                    return false;
                }
            }
            return true;
        }

        private bool Contains<T>(ISet<T> set, T obj)
        {
            foreach (T item in set)
            {
                if (this._PropertyEqualsCalculator.Equals(item, obj))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsApplicable(Type typeOfObject1, Type typeOfObject2)
        {
            return EnumerableTools.TypeIsSet(typeOfObject1) && EnumerableTools.TypeIsSet(typeOfObject2);
        }

        public override int DefaultGetHashCode(object obj)
        {
            return this.Configuration.GetHashCode(obj);
        }
    }
}