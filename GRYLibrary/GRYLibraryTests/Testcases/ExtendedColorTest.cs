using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class ExtendedColorTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExtendedColorTest1()
        {
            ExtendedColor color = new();
            Assert.AreEqual(0, color.ColorCode);
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExtendedColorTest2()
        {
            ExtendedColor color1 = new(42);
            ExtendedColor color2 = new(42);
            Assert.AreEqual(color1, color2);
            Assert.AreEqual(color1.GetHashCode(), color2.GetHashCode());
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExtendedColorTest3()
        {
            byte testValueA = 200;
            byte testValueR = 255;
            byte testValueG = 0;
            byte testValueB = 10;

            ExtendedColor color = new(testValueA, testValueR, testValueG, testValueB);
            Assert.AreEqual(-922812406, color.ColorCode);
            Assert.AreEqual(testValueA, color.A);
            Assert.AreEqual(testValueR, color.R);
            Assert.AreEqual(testValueG, color.G);
            Assert.AreEqual(testValueB, color.B);
            Assert.AreEqual(System.Drawing.Color.FromArgb(testValueA, testValueR, testValueG, testValueB), color.DrawingColor);
            Assert.AreEqual("C8FF000A", color.GetARGBString());
            Assert.AreEqual("FF000A", color.GetRGBString());
            Assert.AreEqual("ExtendedColor(A=200,R=255,G=0,B=10)", color.ToString());
        }
    }
}