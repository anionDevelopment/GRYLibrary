using System;

namespace GRYLibrary.Core.AOA.EqualsHelper
{
    public abstract class AbstractCustomComparer : GRYEqualityComparer<object>
    {
        public abstract bool IsApplicable(Type typeOfObject1, Type typeOfObject2);

        protected AbstractCustomComparer(PropertyEqualsCalculatorConfiguration configuration) : base(configuration)
        {
        }
    }
}