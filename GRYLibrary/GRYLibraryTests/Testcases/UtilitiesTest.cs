using GRYLibrary.Core.Misc;
using GRYLibrary.Core.XMLSerializer;
using GRYLibrary.Tests.TestData.TestTypes.CyclicDataStructure;
using GRYLibrary.Tests.TestData.TestTypes.SimpleDataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void TestResolvePathOfProgram()
        {
            // arrange
            string originalProgram = "git";
            string program = originalProgram;
            string originalArgument = "someargument";
            string argument = originalArgument;

            // act
            Tuple<string, string, string> result = GUtilities.ResolvePathOfProgram(program, argument, null);

            // assert
            Assert.IsTrue(Path.IsPathFullyQualified(result.Item1));
            Assert.IsTrue(program.Contains(originalProgram));
            Assert.AreEqual(argument, originalArgument);
        }

        [TestMethod]
        public void NormalizePathConvertsSeparatorsForTheGivenOperatingSystem()
        {
            // The target-operating-system is passed explicitly (not derived from the host), so this test performs exactly the same operation and expects
            // exactly the same result on Windows and on Linux. It also guards against the regression where backslashes were not converted to '/' for Linux.
            string mixed = "foo\\bar/baz";

            Assert.AreEqual("foo/bar/baz", GUtilities.NormalizePath(mixed, GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Linux.Instance));
            Assert.AreEqual("foo/bar/baz", GUtilities.NormalizePath(mixed, GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.OSX.Instance));
            Assert.AreEqual("foo\\bar\\baz", GUtilities.NormalizePath(mixed, GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows.Instance));
        }

        [TestMethod]
        public void UtilitiesTestEnsureFileExists()
        {
            string testFile = "file";
            try
            {
                Assert.IsFalse(File.Exists(testFile));
                GUtilities.EnsureFileExists(testFile);
                Assert.IsTrue(File.Exists(testFile));
                GUtilities.EnsureFileExists(testFile);
                Assert.IsTrue(File.Exists(testFile));
            }
            finally
            {
                File.Delete(testFile);
            }
        }

        [TestMethod]
        public void UtilitiesTestEnsureFileDoesNotExist()
        {
            string testFile = "file";
            GUtilities.EnsureFileExists(testFile);
            Assert.IsTrue(File.Exists(testFile));
            GUtilities.EnsureFileDoesNotExist(testFile);
            Assert.IsFalse(File.Exists(testFile));
            GUtilities.EnsureFileDoesNotExist(testFile);
            Assert.IsFalse(File.Exists(testFile));
        }

        [TestMethod]
        public void UtilitiesTestEnsureDirectoryExists()
        {
            string testDir = "dir";
            try
            {
                Assert.IsFalse(Directory.Exists(testDir));
                GUtilities.EnsureDirectoryExists(testDir);
                Assert.IsTrue(Directory.Exists(testDir));
                GUtilities.EnsureDirectoryExists(testDir);
                Assert.IsTrue(Directory.Exists(testDir));
            }
            finally
            {
                Directory.Delete(testDir);
            }
        }

        [TestMethod]
        public void UtilitiesTestEnsureDirectoryDoesNotExist()
        {
            string testDir = "dir";
            GUtilities.EnsureDirectoryExists(testDir);
            Assert.IsTrue(Directory.Exists(testDir));
            GUtilities.EnsureDirectoryDoesNotExist(testDir);
            Assert.IsFalse(Directory.Exists(testDir));
            GUtilities.EnsureDirectoryDoesNotExist(testDir);
            Assert.IsFalse(Directory.Exists(testDir));
        }

        [TestMethod]
        public void UtilitiesTestEnsureDirectoryDoesNotExist2()
        {
            string dir = "dir";
            string testFile = dir + "/file";
            GUtilities.EnsureFileExists(testFile, true);
            Assert.IsTrue(File.Exists(testFile));
            GUtilities.EnsureDirectoryDoesNotExist(dir);
            Assert.IsFalse(Directory.Exists(testFile));
        }

        [TestMethod]
        public void FileSelectorTest1()
        {
            string baseDir = "basetestdir/";
            string dir1 = baseDir + "dir1/";
            string dir2 = dir1 + "dir2/";
            string file1 = baseDir + dir1 + "file1";
            string file2 = baseDir + dir2 + "file2";
            string file3 = baseDir + dir2 + "file3";
            string file4 = baseDir + "dir3/file4";
            try
            {
                GUtilities.EnsureFileExists(file1, true);
                GUtilities.EnsureFileExists(file2, true);
                GUtilities.EnsureFileExists(file3, true);
                GUtilities.EnsureFileExists(file4, true);

                IEnumerable<string> result = GUtilities.GetFilesOfFolderRecursively(baseDir);
                Assert.AreEqual(4, result.Count());
            }
            finally
            {
                GUtilities.EnsureDirectoryDoesNotExist(baseDir);
            }
        }

        [TestMethod]
        public void IncrementGuidTest1()
        {
            string input = "5fe3eb8e-39dc-469c-a9cd-ea740e90d338";
            Guid inputId = Guid.Parse(input);
            Guid result = GUtilities.IncrementGuid(inputId);
            Assert.AreEqual("5fe3eb8e-39dc-469c-a9cd-ea740e90d339", result.ToString());
        }

        [TestMethod]
        public void IncrementGuidTest2()
        {
            string input = "0003eb8e-39dc-469c-a9cd-00740e90d338";
            Guid inputId = Guid.Parse(input);
            Guid result = GUtilities.IncrementGuid(inputId);
            Assert.AreEqual("0003eb8e-39dc-469c-a9cd-00740e90d339", result.ToString());
        }

        [TestMethod]
        public void IncrementGuidTest3()
        {
            string input = "0003eb8e-39dc-469c-a9cd-90740e90d338";
            Guid inputId = Guid.Parse(input);
            Guid result = GUtilities.IncrementGuid(inputId, BigInteger.Parse("100000000000", NumberStyles.HexNumber));
            Assert.AreEqual("0003eb8e-39dc-469c-a9cd-a0740e90d338", result.ToString());
        }

        [TestMethod]
        public void IncrementGuidTest4()
        {
            string input = "5fe3eb8e-39dc-469c-a9cd-ea740e90d338";
            Guid inputId = Guid.Parse(input);
            Guid result = GUtilities.IncrementGuid(inputId);
            Assert.AreNotEqual(input, result.ToString());
        }

        [Ignore]
        [TestMethod]
        public void GenericSerializerTest1()
        {
            SimpleDataStructure3 testObject = SimpleDataStructure3.GetRandom();
            SimpleGenericXMLSerializer<SimpleDataStructure3> serializer = new();
            string serialized = serializer.Serialize(testObject);
            SimpleDataStructure3 deserialized = serializer.Deserialize(serialized);
            Assert.AreEqual(testObject, deserialized);
        }

        [TestMethod]
        public void SerializeableDictionaryTest()
        {
            SerializableDictionary<int, string> dictionary = new()
            {
                { 1, "test1" },
                { 2, "test2" }
            };
            SimpleGenericXMLSerializer<SerializableDictionary<int, string>> serializer = new();
            string serializedDictionary = serializer.Serialize(dictionary);
            SerializableDictionary<int, string> reloadedDictionary = serializer.Deserialize(serializedDictionary);
            Assert.AreEqual(2, reloadedDictionary.Count);
            Assert.AreEqual("test1", reloadedDictionary[1]);
            Assert.AreEqual("test2", reloadedDictionary[2]);
        }

        [TestMethod]
        public void IsListTest()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsList(new List<int>()));
            Assert.IsTrue(EnumerableTools.ObjectIsList(new ArraySegment<int>()));
            Assert.IsTrue(EnumerableTools.ObjectIsList(new ArrayList()));
            Assert.IsFalse(EnumerableTools.ObjectIsList(new LinkedList<int>()));
            Assert.IsFalse(EnumerableTools.ObjectIsList(new object()));
            Assert.IsFalse(EnumerableTools.ObjectIsList("somestring"));
            Assert.IsTrue(EnumerableTools.ObjectIsList("somestring".ToCharArray()));
        }

        [TestMethod]
        public void IsPrimitiveTest()
        {
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(true));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(false));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(3));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(0));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive("somestring"));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(1.5));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(Guid.NewGuid()));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(default(Guid)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(default(int)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(default(bool)));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new Exception()));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new Uri("https://example.org")));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new ArraySegment<int>()));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new ArrayList()));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new LinkedList<int>()));
            Assert.IsFalse(GUtilities.ObjectIsPrimitive(new object()));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(typeof(int)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(typeof(object)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(typeof(ArrayList)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(typeof(IList<>)));
            Assert.IsTrue(GUtilities.ObjectIsPrimitive(typeof(IList<Func<Uri>>)));
        }

        [TestMethod]
        public void TypeRepresentsTypeTest()
        {
            Assert.IsFalse(GUtilities.TypeRepresentsType(typeof(object)));
            Assert.IsFalse(GUtilities.TypeRepresentsType(typeof(List<Type>)));
            Assert.IsTrue(GUtilities.TypeRepresentsType(typeof(Type)));
            Assert.IsTrue(GUtilities.TypeRepresentsType(typeof(TypeInfo)));
        }

        [TestMethod]
        public void IsDictionaryEntryTest()
        {
            Assert.IsFalse(EnumerableTools.ObjectIsDictionaryEntry(new List<int>()));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionaryEntry(5));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionaryEntry(new System.Collections.Generic.KeyValuePair<object, object>()));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionaryEntry(new System.Collections.Generic.KeyValuePair<object, object>(new object(), new object())));
            Assert.IsTrue(EnumerableTools.ObjectIsDictionaryEntry(new DictionaryEntry()));
            Assert.IsTrue(EnumerableTools.ObjectIsDictionaryEntry(new DictionaryEntry(new object(), new object())));
        }

        [TestMethod]
        public void IsDictionaryTest()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsDictionary(new Dictionary<int, string>()));
            Assert.IsTrue(EnumerableTools.ObjectIsDictionary(ImmutableDictionary.CreateBuilder<long, object>().ToImmutable()));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionary(new LinkedList<int>()));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionary(new object()));
            Assert.IsFalse(EnumerableTools.ObjectIsDictionary("somestring"));
        }

        [TestMethod]
        public void IsSetTest()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsSet(new HashSet<int>()));
            Assert.IsTrue(EnumerableTools.ObjectIsSet(new SortedSet<string>()));
            Assert.IsFalse(EnumerableTools.ObjectIsSet(new LinkedList<int>()));
            Assert.IsFalse(EnumerableTools.ObjectIsSet(new object()));
            Assert.IsFalse(EnumerableTools.ObjectIsSet("somestring"));
        }

        [TestMethod]
        public void IsKeyValuePairTest11()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(new System.Collections.Generic.KeyValuePair<object, object>(new object(), new object())));
        }

        [TestMethod]
        public void IsKeyValuePairTest12()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(new System.Collections.Generic.KeyValuePair<int, string>()));
        }

        [TestMethod]
        public void IsKeyValuePairTest21()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(new System.Collections.Generic.KeyValuePair<object, object>(new object(), new object())));
        }

        [TestMethod]
        public void IsKeyValuePairTest22()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(new System.Collections.Generic.KeyValuePair<int, string>()));
        }

        [TestMethod]
        public void ObjectToKeyValuePairTest1()
        {
            object kvp11 = new();
            object kvp12 = new();
            object kvp1object = new System.Collections.Generic.KeyValuePair<object, object>(kvp11, kvp12);
            System.Collections.Generic.KeyValuePair<object, object> kvp1Typed = EnumerableTools.ObjectToKeyValuePair<object, object>(kvp1object);
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(kvp1Typed));
            Assert.AreEqual(kvp11, kvp1Typed.Key);
            Assert.AreEqual(kvp12, kvp1Typed.Value);
        }

        [TestMethod]
        public void ObjectToKeyValuePairTest2()
        {
            int kvp11 = 6;
            string kvp12 = "test";
            object kvp1object = new System.Collections.Generic.KeyValuePair<int, string>(kvp11, kvp12);
            System.Collections.Generic.KeyValuePair<object, object> kvp1Typed = EnumerableTools.ObjectToKeyValuePair<object, object>(kvp1object);
            Assert.IsTrue(EnumerableTools.ObjectIsKeyValuePair(kvp1Typed));
            Assert.AreEqual(kvp11, kvp1Typed.Key);
            Assert.AreEqual(kvp12, kvp1Typed.Value);
        }

        [TestMethod]
        public void IsTupleTest11()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(new Tuple<object, object>(new object(), new object())));
        }

        [TestMethod]
        public void IsTupleTest12()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(new WriteableTuple<object, object>()));
        }

        [TestMethod]
        public void IsTupleTest21()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(new Tuple<int, string>(5, "test")));
        }

        [TestMethod]
        public void IsTupleTest22()
        {
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(new WriteableTuple<int, string>()));
        }

        [TestMethod]
        public void ObjectToTupleTest1()
        {
            object kvp11 = new();
            object kvp12 = new();
            object kvp1object = new Tuple<object, object>(kvp11, kvp12);
            Tuple<object, object> kvp1Typed = EnumerableTools.ObjectToTuple<object, object>(kvp1object);
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(kvp1Typed));
            Assert.AreEqual(kvp11, kvp1Typed.Item1);
            Assert.AreEqual(kvp12, kvp1Typed.Item2);
        }

        [TestMethod]
        public void ObjectToTupleTest2()
        {
            int kvp11 = 6;
            string kvp12 = "test";
            object kvp1object = new Tuple<int, string>(kvp11, kvp12);
            Tuple<object, object> kvp1Typed = EnumerableTools.ObjectToTuple<object, object>(kvp1object);
            Assert.IsTrue(EnumerableTools.ObjectIsTuple(kvp1Typed));
            Assert.AreEqual(kvp11, kvp1Typed.Item1);
            Assert.AreEqual(kvp12, kvp1Typed.Item2);
        }

        [TestMethod]
        public void ObjectToSettTest()
        {
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToSet<object>(new object()));
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToSet<object>(5));

            Assert.IsTrue(EnumerableTools.SetEquals(new HashSet<char> { 's', 'o', 'm', 'e', 't' }, EnumerableTools.ObjectToSet<char>(new HashSet<char> { 's', 'o', 'm', 'e', 't', 'e', 's', 't' })));

            HashSet<int> testSet = [3, 4, 5];
            object testSetAsObject = testSet;
            Assert.IsTrue(EnumerableTools.SetEquals(testSet, EnumerableTools.ObjectToSet<int>(testSetAsObject)));
        }

        [TestMethod]
        public void ObjectToListTest()
        {
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToList<object>(new object()));
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToList<object>("sometest"));

            List<int> testList = [3, 4, 5];
            object testListAsObject = testList;
            Assert.IsTrue(EnumerableTools.ListEquals(testList, EnumerableTools.ObjectToList<int>(testListAsObject)));

            Assert.IsTrue(EnumerableTools.ListEquals(testList, (IList)new List<int> { 3, 4, 5 }.ToImmutableList()));
        }

        [TestMethod]
        public void ObjectToDictionaryFailTest()
        {
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToDictionary<object, object>(new object()));
            Assert.Throws<InvalidCastException>(() => EnumerableTools.ObjectToDictionary<object, object>("somestring"));
        }

        [TestMethod]
        public void ObjectToDictionarySuccessTest()
        {
            Dictionary<int, string> testDictionary = new() { { 3, "3s" }, { 4, "4s" }, { 5, "5s" } };
            object testDictionaryAsObject = testDictionary;
            EnumerableTools.ObjectToDictionary<int, string>(testDictionaryAsObject);
        }

        [TestMethod]
        public void DictionaryEqualsFailTest()
        {
            //arrange
            Dictionary<int, string> testDictionary1 = new() { { 3, "3s" }, { 4, "4s" }, { 5, "5s" } };
            Dictionary<int, string> testDictionary2 = new() { { 3, "3s" }, { 4, "4s" } };

            // act && assert
            Assert.IsFalse(EnumerableTools.DictionaryEquals<int, string>(testDictionary1, testDictionary2));
        }

        [TestMethod]
        public void DictionaryEqualsSuccessTest1()
        {
            Dictionary<int, string> testDictionary = new() { { 3, "3s" }, { 4, "4s" }, { 5, "5s" } };
            object testDictionaryAsObject = testDictionary;
            Assert.IsTrue(EnumerableTools.DictionaryEquals(testDictionary, EnumerableTools.ObjectToDictionary<int, string>(testDictionaryAsObject)));
        }

        [TestMethod]
        public void DictionaryEqualsSuccessTest2()
        {
            Dictionary<int, string> testDictionary = new() { { 3, "3s" }, { 4, "4s" }, { 5, "5s" } };
            object testDictionaryAsObject = testDictionary;
            Assert.IsTrue(EnumerableTools.DictionaryEquals(testDictionary, EnumerableTools.ObjectToDictionary<int, string>(testDictionaryAsObject)));

            IDictionary<int, string> testDictionary2 = new ConcurrentDictionary<int, string>();
            testDictionary2.Add(3, "3s");
            testDictionary2.Add(4, "4s");
            testDictionary2.Add(5, "5s");
            object testDictionaryAsObject2 = testDictionary2;
            Assert.IsTrue(EnumerableTools.DictionaryEquals(testDictionary2, EnumerableTools.ObjectToDictionary<int, string>(testDictionaryAsObject2)));

            Assert.IsTrue(EnumerableTools.DictionaryEquals(testDictionary, testDictionary2));
        }

        [TestMethod]
        public void ObjectIsEnumerableTest()
        {
            IEnumerable setAsEnumerable = new HashSet<object> { 3, 4, 5 };
            Assert.IsTrue(EnumerableTools.ObjectIsEnumerable(setAsEnumerable));
            Assert.IsTrue(EnumerableTools.ObjectIsEnumerable(new HashSet<object> { 3, 4, 5 }));
            Assert.IsTrue(EnumerableTools.ObjectIsEnumerable(new HashSet<int> { 3, 4, 5 }));
            Assert.IsTrue(EnumerableTools.ObjectIsEnumerable(new List<SimpleDataStructure3>()));
            Assert.IsTrue(EnumerableTools.ObjectIsEnumerable(string.Empty));
            Assert.IsFalse(EnumerableTools.ObjectIsEnumerable(4));
        }

        [TestMethod]
        public void EnumerableCount()
        {
            List<object> list = [3, 4, 5];
            IEnumerable listAsEnumerable = list;
            Assert.AreEqual(list.Count, GUtilities.Count(listAsEnumerable));
        }

        [TestMethod]
        public void IsAssignableFromTest()
        {
            Assert.IsTrue(GUtilities.IsAssignableFrom(new SimpleDataStructure1(), typeof(SimpleDataStructure1)));
            Assert.IsTrue(GUtilities.IsAssignableFrom(new SimpleDataStructure1(), typeof(IXmlSerializable)));
        }

        [Ignore]
        [TestMethod]
        public void ReferenceEqualsWithCommonValuesTest()
        {
            Guid guid1 = Guid.NewGuid();
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(guid1, guid1));
            Guid guid2 = Guid.NewGuid();
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(guid1, guid2));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(Guid.Parse("33257693-bcee-4afd-a648-dd45ee06695d"), Guid.Parse("33257693-bcee-4afd-a648-dd45ee06695d")));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(Guid.Parse("33257693-bcee-4afd-a648-dd45ee06695d"), Guid.Parse("22257693-bcee-4afd-a648-dd45ee066922")));
            object @object = new();
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(@object, @object));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(@object, new object()));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(@object, "string"));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(5, 5));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals("string", "string"));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals("string", "string2"));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(5, 6));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(5, null));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(null, "string"));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(0, new object()));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(0, null));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(new System.Collections.Generic.KeyValuePair<int, string>(5, "s"), new System.Collections.Generic.KeyValuePair<int, string>(5, "s")));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(new System.Collections.Generic.KeyValuePair<int, string>(5, "s1"), new System.Collections.Generic.KeyValuePair<int, string>(5, "s2")));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(new System.Collections.Generic.KeyValuePair<int, string>(5, "s"), new System.Collections.Generic.KeyValuePair<int, string>(6, "s")));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(new System.Collections.Generic.KeyValuePair<int, string>(5, null), new System.Collections.Generic.KeyValuePair<int, object>(5, null)));
        }

        [TestMethod]
        public void ReferenceEqualsCycleTest1()
        {
            CycleA cycle = CycleA.GetRandom();
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(cycle, cycle));
        }

        [TestMethod]
        public void ReferenceEqualsCycleTest2()
        {
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(CycleA.GetRandom(), CycleA.GetRandom()));
        }

        [TestMethod]
        public void ReferenceEqualsCycleTest3()
        {
            object obj1 = new();
            object obj2 = new();

            WriteableTuple<object, object> wt1 = new();
            WriteableTuple<object, object> wt2 = new();
            WriteableTuple<object, object> wt3 = new();
            WriteableTuple<object, object> wt4 = new();
            WriteableTuple<object, object> wt5 = new();
            WriteableTuple<object, object> wt6 = new();
            WriteableTuple<object, object> wt7 = new();
            WriteableTuple<object, object> wt8 = new();

            System.Collections.Generic.KeyValuePair<object, object> kvp1 = new(wt1, wt2);
            System.Collections.Generic.KeyValuePair<object, object> kvp2 = new(wt3, wt4);
            System.Collections.Generic.KeyValuePair<object, object> kvp3 = new(wt5, wt6);
            System.Collections.Generic.KeyValuePair<object, object> kvp4 = new(wt7, obj1);
            System.Collections.Generic.KeyValuePair<object, object> kvp5 = new(obj1, obj2);
            System.Collections.Generic.KeyValuePair<object, object> kvp6 = new(obj2, wt8);

            wt1.Item1 = kvp2;
            wt2.Item1 = kvp3;
            wt3.Item1 = kvp4;
            wt4.Item1 = kvp5;
            wt5.Item1 = kvp5;
            wt6.Item1 = kvp6;
            wt7.Item1 = kvp1;
            wt8.Item1 = kvp1;

            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp1, kvp1));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp2, kvp2));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp3, kvp3));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp4, kvp4));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp5, kvp5));
            Assert.IsTrue(GUtilities.ImprovedReferenceEquals(kvp6, kvp6));

            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(kvp2, kvp3));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(kvp1, kvp3));
            Assert.IsFalse(GUtilities.ImprovedReferenceEquals(kvp1, kvp4));
        }

        [TestMethod]
        public void EnsurePathHasNoLeadingOrTrailingQuotesTest()
        {
            Assert.AreEqual("a", GUtilities.EnsurePathHasNoLeadingOrTrailingQuotes("'\"'\"a\"'\"'"));
        }

        [TestMethod]
        public void EnsurePathDoesNotHaveLeadingOrTrailingSlashOrBackslashTest()
        {
            Assert.AreEqual("a", GUtilities.EnsurePathEndsWithoutSlashOrBackslash("a/"));
            Assert.AreEqual("a", GUtilities.EnsurePathEndsWithoutSlashOrBackslash("a\\"));
        }

        [TestMethod]
        public void HexStringToByteArrayTest()
        {
            // arrange
            string input = "de";
            byte[] expected = [222];

            // act
            byte[] actual = GUtilities.HexStringToByteArray(input);

            // assert
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void ByteArrayToHexStringTest()
        {
            // arrange
            byte[] input = [222];
            string expected = "DE";

            // act
            string actual = GUtilities.ByteArrayToHexString(input);

            // assert
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void BinaryStringToUintTest1()
        {
            // arrange
            string input = "1001";
            uint expected = 9;

            // act
            uint actual = GUtilities.BinaryStringToUint(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BinaryStringToUintTest2()
        {
            // arrange
            string input = "01011101010";
            uint expected = 746;

            // act
            uint actual = GUtilities.BinaryStringToUint(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BinaryStringToUintTest3()
        {
            // arrange
            string input = "11111011101010101010001001010110";
            uint expected = 4222263894;

            // act
            uint actual = GUtilities.BinaryStringToUint(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UintToBinaryStringTest()
        {
            // arrange
            uint input = 4222263894;
            string expected = "11111011101010101010001001010110";

            // act
            string actual = GUtilities.UintToBinaryString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEncodingTest()
        {
            Assert.AreEqual(new ASCIIEncoding(), GUtilities.GetEncodingByIdentifier("us-ascii"));
            Assert.AreEqual(new UTF8Encoding(false), GUtilities.GetEncodingByIdentifier("utf-8"));
            Assert.AreEqual(new UTF8Encoding(true), GUtilities.GetEncodingByIdentifier("utf-8-bom"));
            Assert.AreEqual(new UnicodeEncoding(false, false), GUtilities.GetEncodingByIdentifier("utf-16"));
            Assert.AreEqual(new UnicodeEncoding(false, true), GUtilities.GetEncodingByIdentifier("utf-16-bom"));
            Assert.AreEqual(new UnicodeEncoding(true, false), GUtilities.GetEncodingByIdentifier("utf-16-be"));
            Assert.AreEqual(new UnicodeEncoding(true, true), GUtilities.GetEncodingByIdentifier("utf-16-be-bom"));
            Assert.AreEqual(new UTF32Encoding(false, false), GUtilities.GetEncodingByIdentifier("utf-32"));
            Assert.AreEqual(new UTF32Encoding(false, true), GUtilities.GetEncodingByIdentifier("utf-32-bom"));
            Assert.AreEqual(new UTF32Encoding(true, false), GUtilities.GetEncodingByIdentifier("utf-32-be"));
            Assert.AreEqual(new UTF32Encoding(true, true), GUtilities.GetEncodingByIdentifier("utf-32-be-bom"));
            Assert.AreEqual(Encoding.GetEncoding("iso-8859-1"), GUtilities.GetEncodingByIdentifier("iso-8859-1"));
        }

        [TestMethod]
        public void UnsignedInteger32BitToByteArrayAndViceVersaTest1()
        {
            // arrange
            uint expected = 4222263891;

            // act
            uint actual = GUtilities.ByteArrayToUnsignedInteger32Bit(GUtilities.UnsignedInteger32BitToByteArray(expected));

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UnsignedInteger32BitToByteArrayAndViceVersaTest2()
        {
            // arrange
            byte[] expected = [1, 34, 241, 25];

            // act
            byte[] actual = GUtilities.UnsignedInteger32BitToByteArray(GUtilities.ByteArrayToUnsignedInteger32Bit(expected));

            // assert
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TypeComparerIgnoringGenericsTest()
        {
            // assert
            Assert.IsTrue(GUtilities.TypeComparerIgnoringGenerics.Equals(typeof(System.Collections.Generic.KeyValuePair<Hashtable, ulong>), typeof(System.Collections.Generic.KeyValuePair<int, string>)));
            Assert.IsFalse(GUtilities.TypeComparerIgnoringGenerics.Equals(typeof(HashSet<int>), typeof(List<int>)));
        }

        [TestMethod]
        [Ignore]
        public void CastTest()
        {
            System.Collections.Generic.KeyValuePair<object, object> testObject = new System.Collections.Generic.KeyValuePair<object, object>(1, 2);
            System.Collections.Generic.KeyValuePair<int, int> expectedObject = new System.Collections.Generic.KeyValuePair<int, int>(1, 2);

            //  dynamic x = (dynamic)testObject;
            //   System.Collections.Generic.KeyValuePair<int, int> actualObjectDirectCasted = (System.Collections.Generic.KeyValuePair<int, int>)x;

            object actualObject = GUtilities.Cast2(testObject, expectedObject.GetType());


            Assert.AreNotEqual(testObject, actualObject);
            Assert.AreEqual(expectedObject, actualObject);
        }

        [TestMethod]
        public void DateAndTimeToStringTest()
        {
            Assert.AreEqual("2022-02-27_15-05-00", GUtilities.DateTimeForFilename(new DateTime(2022, 02, 27, 15, 05, 00)));
            Assert.AreEqual("2022-02-27T15:05:00,120+03:00", GUtilities.DateTimeToISO8601String(new DateTimeOffset(2022, 02, 27, 15, 05, 00, 120,TimeSpan.FromHours(3))));
            Assert.AreEqual("2022-02-27T15:05:00+03:00", GUtilities.DateTimeToUserFriendlyString(new DateTimeOffset(2022, 02, 27, 15, 05, 00, TimeSpan.FromHours(3))));
            Assert.AreEqual("2022-02-27", GUtilities.DateToUserFriendlyString(new DateOnly(2022, 02, 27)));
            Assert.AreEqual("15:05:00", GUtilities.TimeToUserFriendlyString(new TimeOnly(15, 05, 00)));
        }

        [TestMethod]
        public void TestIsAbsolutePath()
        {
            // Windows-drive-letter-paths (like "X:\") are only absolute on Windows.
            // On Linux and macOS an absolute path must start with '/', so these paths are not absolute there.
            if (System.OperatingSystem.IsWindows())
            {
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:\"));
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:\Y\"));
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:\Y\Z"));
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:\Y\Z.mp3"));
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:/Y/Z"));
                Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"X:/Y/Z.mp3"));
            }
            else
            {
                Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"X:/Y/Z"));
                Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"X:/Y/Z.mp3"));
            }
            Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"/X"));
            Assert.IsTrue(GUtilities.IsAbsoluteLocalFilePath(@"/X/Y.mp3"));

            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"example/test"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".git"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".x/y.txt"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"x/y.txt"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"x"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"."));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".."));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"./"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"../"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".\"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"..\"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"./X"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"../X"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".\X"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"..\X"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".\X.mp3"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"..\X.mp3"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@".\X\Y.mp3"));
            Assert.IsFalse(GUtilities.IsAbsoluteLocalFilePath(@"..\X\Y.mp3"));
        }

        [TestMethod]
        public void TestParseDateAmericanFormat()
        {
            // arrange
            string input = "4/3/2017 7:2:53 PM";
            DateTime expected = new DateTime(2017, 4, 3, 19, 2, 53);

            // act
            DateTime actual = GUtilities.ParseDateAmericanFormat(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DecimalToStringTest1()
        {
            // arrange
            decimal input = 4.000m;
            string expected = "4.0";

            // act
            string actual = GUtilities.DecimalToString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DecimalToStringTest2()
        {
            // arrange
            decimal input = 4.05000m;
            string expected = "4.05";

            // act
            string actual = GUtilities.DecimalToString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DecimalToStringTest3()
        {
            // arrange
            decimal input = 4.05m;
            string expected = "4.05";

            // act
            string actual = GUtilities.DecimalToString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DecimalToStringTest4()
        {
            // arrange
            decimal input = 4m;
            string expected = "4.0";

            // act
            string actual = GUtilities.DecimalToString(input);

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}