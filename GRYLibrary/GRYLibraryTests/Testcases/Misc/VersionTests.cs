using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GVersion = GRYLibrary.Core.Misc.Version;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class VersionTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseVersionWithTwoParts()
        {
            // act
            GVersion version = new("1.2");

            // assert
            Assert.AreEqual(1u, version.Major);
            Assert.AreEqual(2u, version.Minor);
            Assert.IsFalse(version.HasBuild);
            Assert.IsFalse(version.HasRevision);
            Assert.AreEqual("1.2", version.ToString());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ParseVersionWithFourParts()
        {
            // act
            GVersion version = new("1.2.3.4");

            // assert
            Assert.AreEqual(3u, version.Build);
            Assert.AreEqual(4u, version.Revision);
            Assert.AreEqual("1.2.3.4", version.ToString());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void NegativeValuesAreNotAllowed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GVersion(-1, 2));
            Assert.Throws<ArgumentException>(() => new GVersion("1.-2"));
        }

        /// <remarks>
        /// A version whose build and revision are not set must also be convertible, because <see cref="System.Version"/> does not accept the value -1.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ToSystemVersionWorksForEveryAmountOfSetParts()
        {
            Assert.AreEqual(new System.Version(1, 2), new GVersion("1.2").ToSystemVersion());
            Assert.AreEqual(new System.Version(1, 2, 3), new GVersion("1.2.3").ToSystemVersion());
            Assert.AreEqual(new System.Version(1, 2, 3, 4), new GVersion("1.2.3.4").ToSystemVersion());
        }

        /// <remarks>
        /// <see cref="System.Version"/> uses -1 as marker for "not set". Such a value must be transferred to "not set" and not be treated as error.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CreateBySystemVersionWithoutBuildAndRevision()
        {
            // act
            GVersion version = new(new System.Version(1, 2));

            // assert
            Assert.IsFalse(version.HasBuild);
            Assert.IsFalse(version.HasRevision);
            Assert.AreEqual("1.2", version.ToString());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CompareVersions()
        {
            Assert.IsTrue(new GVersion("1.2") < new GVersion("1.3"));
            Assert.IsTrue(new GVersion("1.2.3") < new GVersion("1.2.4"));
            Assert.IsTrue(new GVersion("1.2.3.4") > new GVersion("1.2.3.3"));
            Assert.IsTrue(new GVersion("1.2") < new GVersion("1.2.0"));//a version whose build is not set is lower than a version whose build is 0
            Assert.AreEqual(new GVersion("1.2.3.4"), new GVersion("1.2.3.4"));
            Assert.AreNotEqual(new GVersion("1.2"), new GVersion("1.2.0"));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CompareToDoesNotAcceptOtherTypes()
        {
            Assert.AreEqual(1, new GVersion("1.2").CompareTo(null));
            Assert.Throws<ArgumentException>(() => new GVersion("1.2").CompareTo("1.2"));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CloneCreatesAnEqualVersion()
        {
            // arrange
            GVersion version = new("1.2.3.4");

            // act
            GVersion clone = (GVersion)version.Clone();

            // assert
            Assert.AreEqual(version, clone);
            Assert.AreNotSame(version, clone);
        }
    }
}
