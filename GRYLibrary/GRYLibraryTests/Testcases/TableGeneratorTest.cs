using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GRYLibrary.Core.Misc.TableGenerator;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class TableGeneratorTest
    {
        [TestMethod]
        public void TestSimpleASCIITable1()
        {
            string[,] items = new string[3, 4];
            items[0, 0] = "firstline0";
            items[0, 1] = "firstline1_012";
            items[0, 2] = "firstline2";
            items[0, 3] = "firstline3";
            items[1, 0] = "secondline0";
            items[1, 1] = "secondline1";
            items[1, 2] = "secondline2_01";
            items[1, 3] = "secondline3";
            items[2, 0] = "lastline0";
            items[2, 1] = "lastline1";
            items[2, 2] = "lastline2";
            items[2, 3] = "lastline3_012345";
            string[] table = Generate(items, new ASCIITable() { TableHasTitles = true });
            Assert.AreEqual(6, table.Length);
            Assert.AreEqual("┌───────────┬──────────────┬──────────────┬────────────────┐", table[0]);
            Assert.AreEqual("│firstline0 │firstline1_012│firstline2    │firstline3      │", table[1]);
            Assert.AreEqual("├───────────┼──────────────┼──────────────┼────────────────┤", table[2]);
            Assert.AreEqual("│secondline0│secondline1   │secondline2_01│secondline3     │", table[3]);
            Assert.AreEqual("│lastline0  │lastline1     │lastline2     │lastline3_012345│", table[4]);
            Assert.AreEqual("└───────────┴──────────────┴──────────────┴────────────────┘", table[5]);
        }
        [TestMethod]
        public void TestSimpleASCIITable2()
        {
            string[,] items = new string[3, 4];
            items[0, 0] = "firstline0";
            items[0, 1] = "firstline1_012";
            items[0, 2] = "firstline2";
            items[0, 3] = "firstline3";
            items[1, 0] = "secondline";
            items[1, 1] = "secondline1";
            items[1, 2] = "secondline2_01";
            items[1, 3] = "secondline3";
            items[2, 0] = "lastline0";
            items[2, 1] = "lastline1";
            items[2, 2] = "lastline2";
            items[2, 3] = "lastline3_012345";
            string[] table = Generate(items, new ASCIITable() { Characters = new DoubleLineTableCharacter(), MaximalWidth = 11 });
            Assert.AreEqual(5, table.Length);
            Assert.AreEqual("╔══════════╦═══════════╦═══════════╦═══════════╗", table[0]);
            Assert.AreEqual("║firstline0║firstlin...║firstline2 ║firstline3 ║", table[1]);
            Assert.AreEqual("║secondline║secondline1║secondli...║secondline3║", table[2]);
            Assert.AreEqual("║lastline0 ║lastline1  ║lastline2  ║lastline...║", table[3]);
            Assert.AreEqual("╚══════════╩═══════════╩═══════════╩═══════════╝", table[4]);
        }
        [TestMethod]
        public void TestSimpleCSVTable1()
        {
            string[,] items = new string[3, 4];
            items[0, 0] = "firstline0";
            items[0, 1] = "firstline1_012";
            items[0, 2] = "firstline2";
            items[0, 3] = "firstline3";
            items[1, 0] = "secondline";
            items[1, 1] = "secondline1";
            items[1, 2] = "secondline2_01";
            items[1, 3] = "secondline3";
            items[2, 0] = "lastline0";
            items[2, 1] = "lastline1";
            items[2, 2] = "lastline2";
            items[2, 3] = "lastline3_012345";
            string[] table = Generate(items, new CSV());
            Assert.AreEqual(3, table.Length);
            Assert.AreEqual("firstline0;firstline1_012;firstline2;firstline3", table[0]);
            Assert.AreEqual("secondline;secondline1;secondline2_01;secondline3", table[1]);
            Assert.AreEqual("lastline0;lastline1;lastline2;lastline3_012345", table[2]);
        }
    }
}