using GRYLibrary.Core.AOA;
using GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure.PaymentMethods;
using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public abstract class PaymentMethod
    {
        internal static PaymentMethod GetRandom()
        {
            Random random = new();
            double randomValue = random.NextDouble();
            if (randomValue < 0.3)
            {
                return new DebitCard();
            }
            if (randomValue < 0.6)
            {
                return new CreditCard();
            }
            return new Bitcoin();
        }
        #region Overhead
        public override bool Equals(object @object)
        {
            return Generic.GenericEquals(this, @object);
        }
        public override int GetHashCode()
        {
            return Generic.GenericGetHashCode(this);
        }
        public override string ToString()
        {
            return Generic.GenericToString(this);
        }
        #endregion
    }
}