using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class XMLTests
    {
        private readonly Encoding _FileEncoding = new UTF8Encoding(false);
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestXSDValidator1()
        {
            string testDataFolder = Path.Combine("TestData", "XSDValidator", "Test1");
            string testXSD1File = Path.Combine(testDataFolder, "TestXSD1.xsd");
            string testXML1_MatchsXSD1 = Path.Combine(testDataFolder, "TestXML1_MatchsXSD1.xml");
            string testXML2_MatchsXSD1Not = Path.Combine(testDataFolder, "TestXML2_MatchsXSD1Not.xml");

            string xmlWorking = File.ReadAllText(testXML1_MatchsXSD1, this._FileEncoding);
            string xmlNotWorking = File.ReadAllText(testXML2_MatchsXSD1Not, this._FileEncoding);
            string xsd = File.ReadAllText(testXSD1File, this._FileEncoding);
            Assert.IsTrue(Core.Misc.Utilities.ValidateXMLAgainstXSD(xmlWorking, xsd, out _));
            Assert.IsFalse(Core.Misc.Utilities.ValidateXMLAgainstXSD(xmlNotWorking, xsd, out _));
        }
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TestXSLT1()
        {
            string testDataFolder = Path.Combine("TestData", "XSLTTransformator", "Test1");
            string testSource = Path.Combine(testDataFolder, "TestTransformationSource.xml");
            string testTarget = Path.Combine(testDataFolder, "TestTransformationTarget.xml");
            string testXSLT = Path.Combine(testDataFolder, "TestXSLT1.xslt");
            // Specification: line-endings are always LF. Carriage-return-characters get removed so that the comparison is independent of how the test-data-file was checked out.
            string xmlSource = File.ReadAllText(testSource, this._FileEncoding).Replace("\r", string.Empty);
            string xmlTarget = File.ReadAllText(testTarget, this._FileEncoding).Replace("\r", string.Empty);
            string xslt = File.ReadAllText(testXSLT, this._FileEncoding).Replace("\r", string.Empty);
            Assert.AreEqual(xmlTarget, Core.Misc.Utilities.XmlToString(Core.Misc.Utilities.ApplyXSLTToXML(xmlSource, xslt), this._FileEncoding));
        }
    }
}