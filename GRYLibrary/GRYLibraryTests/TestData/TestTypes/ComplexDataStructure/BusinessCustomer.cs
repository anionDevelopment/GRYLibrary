using GRYLibrary.Core.AOA;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class BusinessCustomer : ICustomer
    {
        public string CompanyName { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string Website { get; set; }
        public Address MainResidence { get; set; }

        internal static ICustomer GetRandom()
        {
            BusinessCustomer result = new()
            {
                CompanyName = ComplexDataStructureUtilities.GetRandomName(),
                Website = ComplexDataStructureUtilities.GetRandomWebsite(),
                ContactPhoneNumber = ComplexDataStructureUtilities.GetRandomPhoneNumber(),
                MainResidence = Address.GetRandom()
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