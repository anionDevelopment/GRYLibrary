using GRYLibrary.Core.AOA;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure.PaymentMethods
{
    public class DebitCard : PaymentMethod
    {
        public string DeditCardNumber { get; set; }
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