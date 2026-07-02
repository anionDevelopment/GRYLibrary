using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public MoneyAmount Price { get; set; }
        public string NotesFromCustomer { get; set; }
        public string UsedVoucherCode { get; set; }
        public Address DeliveryAddress { get; set; }

        internal static OrderItem GetRandom(Product product)
        {
            OrderItem result = new()
            {
                Product = product,
                Price = MoneyAmount.GetRandom(),
                NotesFromCustomer = ComplexDataStructureUtilities.GetRandomNotes()
            };
            if (new Random().NextDouble() < 0)
            {
                result.UsedVoucherCode = ComplexDataStructureUtilities.GetRandomName();
            }
            else
            {
                result.UsedVoucherCode = null;
            }
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