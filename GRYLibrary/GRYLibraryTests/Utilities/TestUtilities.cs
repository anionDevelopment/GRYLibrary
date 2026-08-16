using GRYLibrary.Core.AOA;
using GRYLibrary.Core.Crypto;
using GRYLibrary.Core.OperatingSystem;
using GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GRYLibrary.Tests.Utilities
{
    public static class TestUtilities
    {
        internal static void AssertEqual(object expectedObject, object actualObject, bool addDefaultEqualAssertion = true)
        {
            bool expectedObjectIsNull = expectedObject == null;
            bool actualObjectIsNull = actualObject == null;
            if (expectedObjectIsNull && actualObjectIsNull)
            {
                Core.Misc.Utilities.NoOperation();
            }
            if (expectedObjectIsNull && !actualObjectIsNull)
            {
                Assert.Fail("actual object is not null");
            }
            if (!expectedObjectIsNull && actualObjectIsNull)
            {
                Assert.Fail("actual object is null");
            }
            if (!expectedObjectIsNull && !actualObjectIsNull)
            {
                Assert.IsTrue(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
                Assert.AreEqual(Generic.GenericGetHashCode(expectedObject), Generic.GenericGetHashCode(actualObject));

                if (addDefaultEqualAssertion)
                {
                    if (Core.Misc.EnumerableTools.ObjectIsSet(expectedObject))
                    {
                        Assert.IsTrue(Core.Misc.EnumerableTools.ObjectToSet<object>(expectedObject).SetEquals(Core.Misc.EnumerableTools.ObjectToSet<object>(actualObject)), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
                    }
                    else if (Core.Misc.EnumerableTools.ObjectIsList(expectedObject))
                    {
                        Assert.IsTrue(Core.Misc.EnumerableTools.ObjectToList<object>(expectedObject).SequenceEqual(Core.Misc.EnumerableTools.ObjectToList<object>(actualObject)), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
                    }
                    else if (!Core.Misc.EnumerableTools.ObjectIsEnumerable(expectedObject))
                    {
                        Assert.AreEqual(expectedObject, actualObject, Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
                        Assert.AreEqual(expectedObject.GetHashCode(), actualObject.GetHashCode());
                    }
                }
            }
        }
        internal static void AssertNotEqual(object expectedObject, object actualObject)
        {
            bool expectedObjectIsNull = expectedObject == null;
            bool actualObjectIsNull = actualObject == null;
            if (expectedObjectIsNull && actualObjectIsNull)
            {
                Assert.Fail("Both objects are equal");
            }
            if (expectedObjectIsNull && !actualObjectIsNull)
            {
                Core.Misc.Utilities.NoOperation();
            }
            if (!expectedObjectIsNull && actualObjectIsNull)
            {
                Core.Misc.Utilities.NoOperation();
            }
            if (!expectedObjectIsNull && !actualObjectIsNull)
            {
                Assert.IsFalse(Generic.GenericEquals(expectedObject, actualObject), Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
                Assert.AreNotEqual(expectedObject, actualObject, Core.Misc.Utilities.GetAssertionFailMessage(expectedObject, actualObject));
            }
        }
        internal static void AssertPureSHA256ValueIsEqualsToDotNetImplementation(string input)
        {
            AssertSHA256ValueIsEqualsToDotNetImplementation(new SHA256PureCSharp(), input);
        }
        internal static void AssertSHA256ValueIsEqualsToDotNetImplementation(HashAlgorithm algorithmUnderTest, string input)
        {
            AssertHashValueIsEqualsToDotNetImplementation(algorithmUnderTest, new SHA256(), input);
        }
        internal static void AssertHashValueIsEqualsToDotNetImplementation(HashAlgorithm algorithmUnderTest, HashAlgorithm verificationAlgorithm, string input)
        {
            // arrange
            byte[] inputAsByteArray = Core.Misc.Utilities.StringToByteArray(input);
            byte[] expectedResult = verificationAlgorithm.Hash(inputAsByteArray);

            // act
            byte[] actualResult = algorithmUnderTest.Hash(inputAsByteArray);

            // assert
            Assert.IsTrue(expectedResult.SequenceEqual(actualResult));
        }
        public static string GetTimeoutTool()
        {
            return Core.OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(GetTimeoutToolVisitor.Instance);
        }
        private class GetTimeoutToolVisitor : IOperatingSystemVisitor<string>
        {
            public static IOperatingSystemVisitor<string> Instance { get; set; } = new GetTimeoutToolVisitor();

            public string Handle(OSX operatingSystem)
            {
                return "sleep";
            }

            public string Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                return "timeout";
            }

            public string Handle(Linux operatingSystem)
            {
                return "sleep";
            }
        }
    }
}