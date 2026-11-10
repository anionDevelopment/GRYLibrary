using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Playlists.ConcretePlaylistHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Tests.Testcases.Playlists.ConcretePlaylistHandler
{
    [TestClass]
    public class M3uHandlerTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
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
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
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

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DeleteItemsFromPlaylist_FileWithoutLineBreakAtTheEnd_AppendsExclusionLineInANewLine()
        {
            //arrange
            Encoding encoding = new UTF8Encoding(false);
            M3UHandler m3uHandler = new M3UHandler();
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = Path.Combine(tempFolder.Path, "test.m3u");
            File.WriteAllText(m3uFile, "./testfile1.mp3\n./testfile2.mp3", encoding);

            //act
            m3uHandler.DeleteItemsFromPlaylist(m3uFile, new string[] { "./testfile1.mp3" });

            //assert
            Assert.AreEqual("./testfile1.mp3\n./testfile2.mp3\n-./testfile1.mp3\n", File.ReadAllText(m3uFile, encoding));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void AddItemsToPlaylist_ItemWasDeletedBefore_ItemIsContainedAgain()
        {
            //arrange
            Encoding encoding = new UTF8Encoding(false);
            M3UHandler m3uHandler = new M3UHandler();
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = Path.Combine(tempFolder.Path, "test.m3u");
            string song1 = Path.Combine(tempFolder.Path, "testfile1.mp3");
            string song2 = Path.Combine(tempFolder.Path, "testfile2.mp3");
            File.WriteAllText(m3uFile, "./testfile1.mp3\n./testfile2.mp3\n", encoding);
            m3uHandler.DeleteItemsFromPlaylist(m3uFile, new string[] { "./testfile1.mp3" });

            //act
            m3uHandler.AddItemsToPlaylist(m3uFile, new string[] { song1 });

            //assert
            Assert.AreEqual($"./testfile1.mp3\n./testfile2.mp3\n{song1}", File.ReadAllText(m3uFile, encoding));
            (ISet<string> included, ISet<string> excluded) = m3uHandler.GetItemsAndExcludedItems(m3uFile);
            Assert.AreEqual(0, excluded.Count);
            Assert.IsTrue(included.Contains(song1));
            Assert.IsTrue(included.Contains("./testfile2.mp3"));
            Assert.IsFalse(included.Contains(song2));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DeleteItemsFromPlaylist_ItemOfAFolderEntry_ItemIsNotLoadedAnymore()
        {
            //arrange
            using TempFolder tempFolder = new TempFolder();
            string musicFolder = Path.Combine(tempFolder.Path, "Music");
            string song1 = Path.Combine(musicFolder, "testfile1.mp3");
            string song2 = Path.Combine(musicFolder, "testfile2.mp3");
            Directory.CreateDirectory(musicFolder);
            GUtilities.EnsureFileExists(song1);
            GUtilities.EnsureFileExists(song2);
            string m3uFile = Path.Combine(tempFolder.Path, "test.m3u");
            File.WriteAllText(m3uFile, "./Music/", new UTF8Encoding(false));
            PlaylistLoader playlistLoader = new PlaylistLoader();

            //act
            playlistLoader.DeleteItemsFromPlaylist(m3uFile, new string[] { song1 });

            //assert
            CollectionAssert.AreEquivalent(new string[] { song2.Replace('\\', '/') }, playlistLoader.GetSongs(m3uFile).ToList());
        }
    }
}
