using System.Collections.Generic;

namespace GRYLibrary.Core.AOA.EqualsHelper
{
    public abstract class GRYEqualityComparer<T> : IEqualityComparer<T>
    {
        protected readonly GRYEqualityComparer<object> _PropertyEqualsCalculator;
        internal PropertyEqualsCalculatorConfiguration Configuration { get; set; }
        public abstract bool DefaultEquals(T item1, T item2);
        public abstract int DefaultGetHashCode(T @object);
        protected GRYEqualityComparer(PropertyEqualsCalculatorConfiguration configuration)
        {
            this.Configuration = configuration;
            if (this is PropertyEqualsCalculator)
            {
                this._PropertyEqualsCalculator = (PropertyEqualsCalculator)(object)this;
            }
            else
            {
                this._PropertyEqualsCalculator = new PropertyEqualsCalculator(this.Configuration);
            }
        }

        public int GetHashCode(T @object)
        {
            return this.DefaultGetHashCode(@object);
        }

        public bool Equals(T item1, T item2)
        {
            return this.DefaultEquals(item1, item2);
        }
    }
}