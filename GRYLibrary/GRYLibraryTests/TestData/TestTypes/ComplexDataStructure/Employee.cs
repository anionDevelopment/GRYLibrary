using GRYLibrary.Core.AOA;
using System;


namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public class Employee : IPerson
    {
        public string Forename { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address MainResidence { get; set; }
        public OrganizationUnit OrganizationUnit { get; set; }

        public static Employee GetRandom(OrganizationUnit organizationUnit = null)
        {
            Employee result = new()
            {
                OrganizationUnit = organizationUnit,
                Forename = ComplexDataStructureUtilities.GetRandomName(),
                Surname = ComplexDataStructureUtilities.GetRandomName(),
                DateOfBirth = ComplexDataStructureUtilities.GetRandomDateOfBirth(),
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