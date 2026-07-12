using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class Product
    {
        public string Name { get; set; }

        internal static Product GetRandom()
        {
            Product result = new()
            {
                Name = GetRandomName()
            };
            return result;
        }

        private static string GetRandomName()
        {
            return Guid.NewGuid().ToString();
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