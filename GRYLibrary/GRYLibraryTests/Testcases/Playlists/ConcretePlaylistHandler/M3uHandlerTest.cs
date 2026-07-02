using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Playlists.ConcretePlaylistHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Tests.Testcases.Playlists.ConcretePlaylistHandler
{
    [TestClass]
    public class M3uHandlerTest
    {
        [TestMethod]
        public void AddSongTo3UTest1()
        {
            //arrange
            Encoding encoding = new UTF8Encoding();
            M3UHandler m3uHandler = new M3UHandler();
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = Path.Combine(tempFolder.Path, "test.mp3");
            GUtilities.EnsureFileExists(m3uFile);
            string content = "./testfile1.mp3";
            File.WriteAllText(m3uFile, content, encoding);
            string newContent = "./testfile2.mp3";
            string expectedConent = $"{content}\n{newContent}";

            //act
            m3uHandler.AddItemsToPlaylist(m3uFile, new string[] { newContent });

            //assert
            string actualContent = File.ReadAllText(m3uFile, encoding);
            Assert.AreEqual(expectedConent, actualContent);
        }

        [TestMethod]
        public void AddSongTo3UTest2()
        {
            //arrange
            Encoding encoding = new UTF8Encoding();
            M3UHandler m3uHandler = new M3UHandler();
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = Path.Combine(tempFolder.Path, "test.mp3");
            GUtilities.EnsureFileExists(m3uFile);
            string content = "./testfile1.mp3\n";
            File.WriteAllText(m3uFile, content, encoding);
            string newContent = "./testfile2.mp3";
            string expectedConent = $"{content}{newContent}";

            //act
            m3uHandler.AddItemsToPlaylist(m3uFile, new string[] { newContent });

            //assert
            string actualContent = File.ReadAllText(m3uFile, encoding);
            Assert.AreEqual(expectedConent, actualContent);
        }
    }
}
