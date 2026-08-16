using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class SimpleObjectPersistenceTest
    {
        [TestMethod]
        public void SimpleObjectPersistenceTestTest1()
        {
            string file = "file.xml";
            try
            {
                Core.Misc.Utilities.EnsureFileDoesNotExist(file);

                Encoding encoding = new UTF8Encoding(false);
                SimpleObjectPersistence<SerializeTestClass> sop1 = SimpleObjectPersistence<SerializeTestClass>.CreateByFile(file);

                SerializeTestClass testObject = SerializeTestClass.CreateTestObject();
                sop1.Object = testObject;
                sop1.SaveObjectToFile();

                string content = System.IO.File.ReadAllText(file, encoding);

                SimpleObjectPersistence<SerializeTestClass> sop2 = SimpleObjectPersistence<SerializeTestClass>.CreateByFile(file);
                sop2.LoadObjectFromFile();

                Assert.AreEqual(3, sop2.Object.ListTest.Count);
                Assert.AreEqual(true, sop2.Object.ListTest[0]);
                Assert.AreEqual(false, sop2.Object.ListTest[1]);
                Assert.AreEqual(true, sop2.Object.ListTest[2]);
                Assert.AreEqual(null, sop2.Object.TestAttribute.TestAttribute1.TestAttribute1);
                Assert.AreEqual("x", sop2.Object.TestAttribute.TestAttribute1.TestString1);
                Assert.AreEqual("encodingtest: áä?<👍你好", sop2.Object.TestAttribute.TestString1);
                Assert.AreEqual(22 / (double)7, sop2.Object.TestDouble);
                Assert.AreEqual(5, sop2.Object.TestDouble2);
                Assert.AreEqual("y", sop2.Object.TestStringFromInterface);

                Assert.AreEqual(testObject, sop2.Object);
                string expectedXMLValue = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SimpleObjectPersistenceTest-SerializeTestClass xmlns:sys=""https://extendedxmlserializer.github.io/system"" xmlns:exs=""https://extendedxmlserializer.github.io/v2"" TestStringFromInterface=""y"" TestDouble=""3.142857142857143"" TestDouble2=""5"">
  <ListTest Capacity=""4"">
    <sys:boolean>true</sys:boolean>
    <sys:boolean>false</sys:boolean>
    <sys:boolean>true</sys:boolean>
  </ListTest>
  <TestAttribute TestString1=""encodingtest: áä?&lt;👍你好"">
    <TestAttribute1 TestString1=""x"" />
  </TestAttribute>
</SimpleObjectPersistenceTest-SerializeTestClass>";
                // The serializer writes with Environment.NewLine; the expected literal is always stored with
                // LF, so the file-content is normalized to LF to keep the comparison OS-independent.
                Assert.AreEqual(expectedXMLValue, content.Replace("\r\n", "\n"));
            }
            finally
            {
                Core.Misc.Utilities.EnsureFileDoesNotExist(file);
            }
        }
        #region TestClass
        public class SerializeTestClass : SerializeTestBaseClass, ISerializeTestInterface
        {
            public string TestStringFromInterface { get; set; }
            public double TestDouble { get; set; }
            public double? TestDouble2 { get; set; }

            public static SerializeTestClass CreateTestObject()
            {
                SerializeTestClass result = new();

                result.ListTest.Add(true);
                result.ListTest.Add(false);
                result.ListTest.Add(true);
                result.TestAttribute = new SerializeTestAttributeClass
                {
                    TestAttribute1 = new SerializeTestAttributeClass
                    {
                        TestAttribute1 = null,
                        TestString1 = "x"
                    },
                    TestString1 = "encodingtest: áä?<👍你好"
                };
                result.TestDouble = 22 / (double)7;
                result.TestDouble2 = 5;
                result.TestStringFromInterface = "y";

                return result;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                {
                    return false;
                }
                SerializeTestClass typedObject = obj as SerializeTestClass;
                return this.TestStringFromInterface.Equals(typedObject.TestStringFromInterface)
                    && this.TestDouble.Equals(typedObject.TestDouble)
                    && this.TestDouble2.Equals(typedObject.TestDouble2);
            }
            public override int GetHashCode()
            {
                return System.HashCode.Combine(this.TestDouble);
            }
        }
        public interface ISerializeTestInterface
        {
            string TestStringFromInterface { get; set; }
        }
        public class SerializeTestBaseClass
        {
            public List<bool> ListTest { get; set; }
            public SerializeTestAttributeClass TestAttribute { get; set; }
            public SerializeTestBaseClass()
            {
                this.ListTest = [];
            }
            public override bool Equals(object obj)
            {
                SerializeTestBaseClass typedObject = obj as SerializeTestBaseClass;
                return this.TestAttribute.Equals(typedObject.TestAttribute)
                    && this.ListTest.SequenceEqual(typedObject.ListTest);
            }
            public override int GetHashCode()
            {
                return System.HashCode.Combine(this.TestAttribute);
            }
        }
        public class SerializeTestAttributeClass
        {
            public string TestString1 { get; set; }
            public SerializeTestAttributeClass TestAttribute1 { get; set; }

            public override bool Equals(object obj)
            {
                SerializeTestAttributeClass typedObject = obj as SerializeTestAttributeClass;
                bool testAttribute1IsNull1 = this.TestAttribute1 == null;
                bool testAttribute1IsNull2 = typedObject.TestAttribute1 == null;

                if (testAttribute1IsNull1 == testAttribute1IsNull2)
                {
                    if (!testAttribute1IsNull1 && !this.TestAttribute1.Equals(typedObject.TestAttribute1))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return this.TestString1.Equals(typedObject.TestString1);
            }
            public override int GetHashCode()
            {
                return System.HashCode.Combine(this.TestString1);
            }
        }
        #endregion 
    }
}