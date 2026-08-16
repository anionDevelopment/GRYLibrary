using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class MoneyAmount
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        internal static MoneyAmount GetRandom()
        {
            MoneyAmount result = new()
            {
                Amount = (decimal)new Random().NextDouble() * 500,
                Currency = "USD"
            };
            return result;
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