using GRYLibrary.Core.AOA;
using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class PrivateCustomer : ICustomer, IPerson
    {
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string ContactPhoneNumber { get; set; }
        public Address MainResidence { get; set; }
        public DateTime DateOfBirth { get; set; }

        internal static ICustomer GetRandom()
        {
            PrivateCustomer result = new()
            {
                Forename = ComplexDataStructureUtilities.GetRandomName(),
                Surname = ComplexDataStructureUtilities.GetRandomName(),
                ContactPhoneNumber = ComplexDataStructureUtilities.GetRandomPhoneNumber(),
                MainResidence = Address.GetRandom(),
                DateOfBirth = ComplexDataStructureUtilities.GetRandomDateOfBirth()
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