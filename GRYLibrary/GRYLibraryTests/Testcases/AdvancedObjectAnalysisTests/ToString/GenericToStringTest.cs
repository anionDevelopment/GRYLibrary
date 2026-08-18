using GRYLibrary.Core.Misc;
using GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.AdvancedObjectAnalysisTests.ToString
{
    [TestClass]
    public class GenericToStringTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SimpleDataStructureTestObjectToString()
        {
            // arrange
            SimpleDataStructure1 testObject = SimpleDataStructure1.GetRandom();
            string expectedString = @"{ (ObjectId: 1, Type: GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure.SimpleDataStructure1) 
  Property1: 
  [
    { (ObjectId: 3, Type: GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure.SimpleDataStructure3) 
      Property4: 
      (Type: String, Value: ""Property4_e7df34db-bb6f-4a11-8c6d-66bccafbd041"")
      Property5: 
      [
        { (ObjectId: 5, Type: GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure.SimpleDataStructure2) 
          Guid: 
          (Type: Guid, Value: ""a54f4945-e928-4296-bf9b-e9ae16b35744"")
        },
        { (ObjectId: 6, Type: GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure.SimpleDataStructure2) 
          Guid: 
          (Type: Guid, Value: ""1735ece2-942f-4380-aec4-27aaa4021ed5"")
        }
      ]
    }
  ]
  Property2: 
  { (ObjectId: 7, Type: GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure.SimpleDataStructure2) 
    Guid: 
    (Type: Guid, Value: ""3735ece2-942f-4380-aec4-27aaa4021ed5"")
  }
  Property3: 
  (Type: Int32, Value: ""21"")
}";

            // act
            string actualString = testObject.ToString();

            // assert
            // The output uses Environment.NewLine; the expected literal is always stored with LF, so the
            // actual output is normalized to LF to keep the comparison OS-independent.
            Assert.AreEqual(expectedString, actualString.Replace("\r\n", "\n"));
        }
    }
}