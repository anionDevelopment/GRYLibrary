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
        /// <summary>
        /// Returns a program and the argument for it which lets the program run for at least the given amount of
        /// seconds. The returned program is available on the current operating-system without an additional
        /// installation.
        /// </summary>
        public static (string Program, string Argument) GetLongRunningProgram(uint durationInSeconds)
        {
            return Core.OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new GetLongRunningProgramVisitor(durationInSeconds));
        }
        private class GetLongRunningProgramVisitor : IOperatingSystemVisitor<(string Program, string Argument)>
        {
            private readonly uint _DurationInSeconds;

            public GetLongRunningProgramVisitor(uint durationInSeconds)
            {
                this._DurationInSeconds = durationInSeconds;
            }

            public (string Program, string Argument) Handle(OSX operatingSystem)
            {
                return ("sleep", this._DurationInSeconds.ToString());
            }

            /// <remarks>
            /// Windows does not have a "sleep"-program, and the "timeout"-program of Windows aborts immediately if its
            /// standard-input is not a console, which is the case when it is started by a testrunner. Therefore a ping
            /// against the loopback-address is used, which waits one second between two attempts.
            /// </remarks>
            public (string Program, string Argument) Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                return ("ping", $"-n {this._DurationInSeconds + 1} 127.0.0.1");
            }

            public (string Program, string Argument) Handle(Linux operatingSystem)
            {
                return ("sleep", this._DurationInSeconds.ToString());
            }
        }
    }
}