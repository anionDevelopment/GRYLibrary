using System;

namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure
{
    public interface IPerson
    {
        public string Forename { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address MainResidence { get; set; }
    }
}