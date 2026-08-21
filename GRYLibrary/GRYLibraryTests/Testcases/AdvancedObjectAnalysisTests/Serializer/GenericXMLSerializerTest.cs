using GRYLibrary.Core.Misc;
using GRYLibrary.Core.AOA;
using GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure;
using GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure2;
using GRYLibrary.Tests.TestData.TestTypes.CyclicDataStructure;
using GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure;
using GRYLibrary.Tests.TestData.TestTypes.XMLSerializableType;
using GRYLibrary.Tests.TestData.TypeWithCommonInterfaces;
using GRYLibrary.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GRYLibrary.Tests.Testcases.AdvancedObjectAnalysisTests.Serializer
{
    [TestClass]
    public class GenericXMLSerializerTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeSimpleTestObject()
        {
            GenericXMLSerializer<object> serializer = GenericXMLSerializer.DefaultInstance();
            SimpleDataStructure1 testObject = SimpleDataStructure1.GetRandom();
            string serialized = serializer.Serialize(testObject);
            SimpleDataStructure1 actualObject = serializer.Deserialize<SimpleDataStructure1>(serialized);
            Assert.AreEqual(testObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeComplexTestObject1()
        {
            // arrange
            GenericXMLSerializer<object> serializer = GenericXMLSerializer.DefaultInstance();
            object expectedObject = CycleA.GetRandom();

            // act
            string serialized = serializer.Serialize(expectedObject);
            CycleA actualObject = serializer.Deserialize<CycleA>(serialized);

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeCyclicTestObject3()
        {
            // arrange
            GenericXMLSerializer<object> serializer = GenericXMLSerializer.DefaultInstance();
            object expectedObject = CycleA.GetRandom();

            // act
            string serialized = serializer.Serialize(expectedObject);
            CycleA actualObject = serializer.Deserialize<CycleA>(serialized);

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeComplexTestObject2()
        {
            // arrange
            object expectedObject = Company.GetRandom();

            // act
            string serialized = Generic.GenericSerialize(expectedObject);
            Company actualObject = Generic.GenericDeserialize<Company>(serialized);

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeCyclicTestObject4()
        {
            // arrange
            object expectedObject = CycleA.GetRandom();

            // act
            string serialized = Generic.GenericSerialize(expectedObject);
            CycleA actualObject = Generic.GenericDeserialize<CycleA>(serialized);

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationListGeneric()
        {
            // arrange
            List<string> expectedObject = ["aö", "b", "\\", "<", "<test>x</test", "&", "nulltest:", null];
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            List<string> actualObject = [];

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serializedObject));
            Assert.AreEqual(8, actualObject.Count);
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationListNotGeneric()
        {
            // arrange
            ArrayList expectedObject = ["aö", "b", "\\", "<", "<test>x</test", "&", "nulltest:", null];
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            ArrayList actualObject = [];

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(8, actualObject.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationSetGeneric()
        {
            // arrange
            ISet<string> expectedObject = new HashSet<string>() { "aö", "b", "\\", "<", "<test>x</test", "&", "nulltest:", null };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            HashSet<string> actualObject = [];

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(8, actualObject.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }


        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithList()
        {
            // arrange
            TypeWithList expectedObject = new() { List = new List<string> { "a", "b" } };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithList actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, actualObject.List.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithListGeneric()
        {
            // arrange
            TypeWithListGeneric<string> expectedObject = new() { List = ["a", "b"] };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithListGeneric<string> actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, actualObject.List.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithEnumerable()
        {
            // arrange
            TypeWithEnumerable expectedObject = new() { Enumerable = new List<string> { "a", "b" } };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithEnumerable actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, Core.Misc.Utilities.Count(actualObject.Enumerable));
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithEnumerableGeneric()
        {
            // arrange
            TypeWithEnumerableGeneric<string> expectedObject = new() { Enumerable = new HashSet<string> { "a", "b" } };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithEnumerableGeneric<string> actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, actualObject.Enumerable.Count());
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithSetGeneric()
        {
            // arrange
            TypeWithSetGeneric<string> expectedObject = new() { Set = new HashSet<string> { "a", "b" } };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithSetGeneric<string> actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, actualObject.Set.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IXmlSerializableDefaultImplementationTypeWithDictionaryGeneric()
        {
            // arrange
            TypeWithDictionaryGeneric<int, string> expectedObject = new() { Dictionary = new Dictionary<int, string>() { { 5, "b" }, { 6, "d" } } };
            StringWriter stringWriter = new();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            TypeWithDictionaryGeneric<int, string> actualObject = new();

            // act
            Generic.GenericWriteXml(expectedObject, writer);
            string serializedObject = stringWriter.ToString();
            Generic.GenericReadXml(actualObject, XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(2, actualObject.Dictionary.Count);
            Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeTypeWithCommonInterfaces1()
        {
            // arrange
            TypeWithCommonInterfaces expectedObject = new()
            {
                List = [2, 4, 3],
                Enumerable = new List<int>() { 2, 4, 3 },
                Set = new HashSet<int>() { 2, 4, 3 },
                Dictionary = new Dictionary<int, int>() { { 2, 20 }, { 4, 40 }, { 3, 30 } }
            };
            StringWriter stringWriter = new();
            XmlWriter xmlWriter = XmlWriter.Create(stringWriter);

            // act
            expectedObject.WriteXml(xmlWriter);
            string serializedObject = stringWriter.ToString();
            TypeWithCommonInterfaces actualObject = new();
            actualObject.ReadXml(XmlReader.Create(new StringReader(serializedObject)));

            // assert
            Assert.AreEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SerializeTypeWithCommonInterfaces2()
        {
            // arrange
            TypeWithCommonInterfaces expectedObject = new()
            {
                List = [2, 4, 3],
                Enumerable = new List<int>() { 2, 4, 3 },
                Set = new HashSet<int>() { 2, 4, 3 },
                //Array = new string[] { "s1", "s2" },
                Dictionary = new Dictionary<int, int>() { { 2, 20 }, { 4, 40 }, { 3, 30 } },
            };
            expectedObject.List.Add(expectedObject);

            // act
            string serialized = Generic.GenericSerialize(expectedObject);
            TypeWithCommonInterfaces actualObject = Generic.GenericDeserialize<TypeWithCommonInterfaces>(serialized);

            // assert
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ComplexDataStructure2EqualType()
        {
            // arrange
            OtherType t1 = new() { Id = Guid.Parse("8e6ce36f-3ce4-4d2f-bff8-6e78db373001"), Name = "type_1" };
            DotNetType t2_1 = new() { Id = Guid.Parse("8e6ce36f-3ce4-4d2f-bff8-6e78db373002"), Name = "type_2", Type = typeof(bool) };
            DotNetType t2_2 = new() { Id = Guid.Parse("8e6ce36f-3ce4-4d2f-bff8-6e78db373002"), Name = "type_2", Type = typeof(bool) };
            DotNetType t3 = new() { Id = Guid.Parse("8e6ce36f-3ce4-4d2f-bff8-6e78db373003"), Name = "type_3", Type = typeof(bool) };
            DotNetType t4 = new() { Id = Guid.Parse("8e6ce36f-3ce4-4d2f-bff8-6e78db373004"), Name = "type_4", Type = typeof(int) };

            // act & assert
            Assert.IsTrue(t1.Equals(t1));
            Assert.IsFalse(t1.Equals(t2_1));
            Assert.IsFalse(t1.Equals(t2_2));
            Assert.IsFalse(t1.Equals(t3));
            Assert.IsFalse(t1.Equals(t4));

            Assert.IsFalse(t2_1.Equals(t1));
            Assert.IsTrue(t2_1.Equals(t2_1));
            Assert.IsTrue(t2_1.Equals(t2_2));
            Assert.IsFalse(t2_1.Equals(t3));
            Assert.IsFalse(t2_1.Equals(t4));

            Assert.IsFalse(t2_2.Equals(t1));
            Assert.IsTrue(t2_2.Equals(t2_1));
            Assert.IsTrue(t2_2.Equals(t2_2));
            Assert.IsFalse(t2_2.Equals(t3));
            Assert.IsFalse(t2_2.Equals(t4));

            Assert.IsFalse(t3.Equals(t1));
            Assert.IsFalse(t3.Equals(t2_1));
            Assert.IsFalse(t3.Equals(t2_2));
            Assert.IsTrue(t3.Equals(t3));
            Assert.IsFalse(t3.Equals(t4));

            Assert.IsFalse(t4.Equals(t1));
            Assert.IsFalse(t4.Equals(t2_1));
            Assert.IsFalse(t4.Equals(t2_2));
            Assert.IsFalse(t4.Equals(t3));
            Assert.IsTrue(t4.Equals(t4));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ComplexDataStructure2Serialize()
        {
            // arrange
            Model expectedObject = new();
            expectedObject.Types.Add(new OtherType() { });
            expectedObject.Types.Add(new OtherType() { Name = "type_2" });
            expectedObject.Types.Add(new OtherType() { Name = "type_3" });
            expectedObject.Types.Add(new OtherType() { Id = Guid.NewGuid(), Name = "type_4" });
            expectedObject.Types.Add(new DotNetType() { Type = typeof(bool) });
            expectedObject.Types.Add(new DotNetType() { Type = typeof(IList<>) });
            expectedObject.Types.Add(new DotNetType() { Id = Guid.NewGuid(), Name = "type_7", Type = typeof(bool) });
            expectedObject.Types.Add(new DotNetType() { Id = Guid.Parse("7e6ce36f-3ce4-4d2f-bff8-6e78db373000"), Name = "type_8", Type = typeof(Func<List<ArrayList>, Model>) });
            expectedObject.Types.Add(new DotNetType() { Id = Guid.Parse("7e6ce36f-3ce4-4d2f-bff8-6e78db373000"), Name = "type_8", Type = typeof(Func<List<ArrayList>, Model>) });//duplicate to test if it will get removed
            Assert.AreEqual(8, expectedObject.Types.Count);

            // act
            string serialized = Generic.GenericSerialize(expectedObject);
            Model actualObject = Generic.GenericDeserialize<Model>(serialized);

            // assert
            Assert.AreEqual(8, actualObject.Types.Count);// Problem here: the desrializing-function fills the 8 !empty! type-objects in the .Types-list, then the HashSet obviously removes them because the properties which define differences between this 8 objects are not set yet
            Assert.IsTrue(Core.Misc.Utilities.IsValidXML(serialized));
            TestUtilities.AssertEqual(expectedObject, actualObject);
        }
    }
}