using GRYLibrary.Core.AOA.EqualsHelper;
using GRYLibrary.Core.Misc;
using System;

namespace GRYLibrary.Core.AOA
{
    public class PropertyEqualsCalculator : GRYEqualityComparer<object>
    {

        public PropertyEqualsCalculator() : this(new PropertyEqualsCalculatorConfiguration())
        {
        }
        internal PropertyEqualsCalculator(PropertyEqualsCalculatorConfiguration configuration) : base(configuration)
        {
        }

        /// <remarks>This function assumes that 2 objects which are not implementing <see cref="System.Collections.IEnumerable"/> are not equal if their types are not equal (except they are enumerables).</remarks>
        public override bool DefaultEquals(object object1, object object2)
        {
            if (this.Configuration.ArePending(object1, object2))
            {
                return true;
            }
            if (this.Configuration.WereMarkedAsNotEqual(object1, object2))
            {
                return false;
            }
            this.Configuration.AddPending(object1, object2);
            if (ReferenceEquals(object1, object2))
            {
                this.MarkedAsEqual(object1, object2);
                return true;
            }
            bool object1IsDefault = Utilities.IsDefault(object1);
            bool object2IsDefault = Utilities.IsDefault(object2);
            if (object1IsDefault && object2IsDefault)
            {
                this.MarkedAsEqual(object1, object2);
                return true;
            }
            else if (!object1IsDefault && !object2IsDefault)
            {
                if (this.Configuration.AreInSameEquivalenceClass(object1, object2))
                {
                    //objects where already compared and it was determined that they are equal
                    this.MarkedAsEqual(object1, object2);
                    return true;
                }
                else if (this.CustomComparerShouldBeApplied(this.Configuration, object1.GetType(), object2.GetType(), out AbstractCustomComparer customComparer))
                {
                    //use custom comparer
                    bool result = customComparer.Equals(object1, object2);
                    if (result)
                    {
                        this.MarkedAsEqual(object1, object2);
                    }
                    else
                    {
                        this.MarkedAsNotEqual(object1, object2);
                    }
                    return result;
                }
                else
                {
                    this.MarkedAsNotEqual(object1, object2);
                    return false;
                }
            }
            else
            {
                this.MarkedAsNotEqual(object1, object2);
                return false;
            }
        }

        private void MarkedAsNotEqual(object object1, object object2)
        {
            this.Configuration.MarkedAsNotEqual(object1, object2);
        }

        private void MarkedAsEqual(object object1, object object2)
        {
            this.Configuration.AddEqualObjectsToEquivalenceClasses(object1, object2);
        }

        private bool CustomComparerShouldBeApplied(PropertyEqualsCalculatorConfiguration configurationAndCache, Type object1Type, Type object2Type, out AbstractCustomComparer customComparer)
        {
            foreach (AbstractCustomComparer comparer in configurationAndCache.CustomComparer)
            {
                if (comparer.IsApplicable(object1Type, object2Type))
                {
                    customComparer = comparer;
                    return true;
                }
            }
            customComparer = null;
            return false;
        }
        public override int DefaultGetHashCode(object obj)
        {
            return this.Configuration.GetHashCode(obj);
        }
    }
}