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
            // A set states no order, so every element of the one set has to be searched in the other one. The elements
            // of that other set are grouped by their hash-code once, so the search starts with the elements which can
            // be the searched one instead of comparing against everything (see Contains).
            Dictionary<int, List<T>> elementsByTheirHashCode = new Dictionary<int, List<T>>();
            foreach (T item in set2)
            {
                int hashCode = Generic.GenericGetHashCode(item);
                if (!elementsByTheirHashCode.TryGetValue(hashCode, out List<T> elementsWithThatHashCode))
                {
                    elementsWithThatHashCode = new List<T>();
                    elementsByTheirHashCode.Add(hashCode, elementsWithThatHashCode);
                }
                elementsWithThatHashCode.Add(item);
            }
            foreach (T obj in set1)
            {
                if (!this.Contains(elementsByTheirHashCode, obj))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// States whether one of the given elements equals the given object.
        /// </summary>
        /// <remarks>
        /// The elements which share the hash-code of the searched object are compared first, because that is where an
        /// equal element is. The remaining ones are compared afterwards and not skipped: a hash-code which differs is
        /// no proof that two objects differ, and this comparison decides what equal means - not the hash-code.
        /// </remarks>
        private bool Contains<T>(Dictionary<int, List<T>> elementsByTheirHashCode, T obj)
        {
            int hashCodeOfTheSearchedObject = Generic.GenericGetHashCode(obj);
            if (elementsByTheirHashCode.TryGetValue(hashCodeOfTheSearchedObject, out List<T> elementsWithTheSameHashCode) && this.ContainsInList(elementsWithTheSameHashCode, obj))
            {
                return true;
            }
            foreach (KeyValuePair<int, List<T>> entry in elementsByTheirHashCode)
            {
                if (entry.Key != hashCodeOfTheSearchedObject && this.ContainsInList(entry.Value, obj))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ContainsInList<T>(List<T> elements, T obj)
        {
            foreach (T item in elements)
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