using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class ColorGradientTest
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void ColorGradientTest1()
        {
            ExtendedColor startColor = new(255, 142, 0);
            ExtendedColor endColor = new(100, 100, 100);
            ColorGradient colorGradient = new(startColor, endColor);
            Assert.AreEqual(startColor, colorGradient.GetColorGradientValue(0));
            Assert.AreEqual(new ExtendedColor(255, 253, 142, 1), colorGradient.GetColorGradientValue(0.01));
            Assert.AreEqual(new ExtendedColor(255, 252, 141, 2), colorGradient.GetColorGradientValue(0.02));
            Assert.AreEqual(new ExtendedColor(255, 248, 140, 5), colorGradient.GetColorGradientValue(0.046));
            Assert.AreEqual(new ExtendedColor(255, 178, 121, 50), colorGradient.GetColorGradientValue(0.50));
            Assert.AreEqual(new ExtendedColor(255, 108, 102, 95), colorGradient.GetColorGradientValue(0.95));
            Assert.AreEqual(endColor, colorGradient.GetColorGradientValue(1));
            Assert.AreEqual(new ExtendedColor(255, 253, 142, 1), colorGradient.GetColorGradientValue(0.01));
        }
    }
}