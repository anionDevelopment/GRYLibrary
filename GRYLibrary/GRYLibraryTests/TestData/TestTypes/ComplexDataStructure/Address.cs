using GRYLibrary.Core.AOA;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Building { get; set; }
        public string City { get; set; }
        public string CountryRegion { get; set; }
        public string FloorLevel { get; set; }
        public string PostalCode { get; set; }
        public string StateProvince { get; set; }
        public string Country { get; set; }

        public static Address GetRandom()
        {
            Address result = new()
            {
                AddressLine1 = ComplexDataStructureUtilities.GetRandomAddressLine1(),
                AddressLine2 = ComplexDataStructureUtilities.GetRandomAddressLine2(),
                Building = ComplexDataStructureUtilities.GetRandomBuilding(),
                City = ComplexDataStructureUtilities.GetRandomCity(),
                CountryRegion = ComplexDataStructureUtilities.GetRandomCountryRegion(),
                FloorLevel = ComplexDataStructureUtilities.GetRandomFloorLevel(),
                PostalCode = ComplexDataStructureUtilities.GetRandomPostalCode(),
                StateProvince = ComplexDataStructureUtilities.GetRandomStateProvince(),
                Country = ComplexDataStructureUtilities.GetRandomCountry()
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