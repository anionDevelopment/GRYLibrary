using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Playlists.ConcretePlaylistHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Tests.Testcases.Playlists.ConcretePlaylistHandler
{
    [TestClass]
    public class PlaylistLoaderTest
    {
        private const string _StreamLink = "https://stream.example.com/radio";

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_EmptyPlaylist_ReturnsNoSongs()
        {
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = WriteFile(tempFolder, "test.m3u", string.Empty);

            Assert.AreEqual(0, new PlaylistLoader().GetSongs(m3uFile).Count);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_M3UWithOnlyOneStreamLink_ReturnsTheLink()
        {
            using TempFolder tempFolder = new TempFolder();
            string m3uFile = WriteFile(tempFolder, "test.m3u", $"#EXTM3U\n{_StreamLink}\n");

            CollectionAssert.AreEqual(new string[] { _StreamLink }, new PlaylistLoader().GetSongs(m3uFile).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_M3UWithRemoteFilesAndLocalFile_ReturnsAllFiles()
        {
            using TempFolder tempFolder = new TempFolder();
            string song1 = WriteFile(tempFolder, "testfile1.mp3", string.Empty);
            string m3uFile = WriteFile(tempFolder, "test.m3u", "./testfile1.mp3\nhttps://files.example.com/a.mp3\nhttps://files.example.com/b.mp3?token=1\n");

            CollectionAssert.AreEqual(new string[] { song1.Replace('\\', '/'), "https://files.example.com/a.mp3", "https://files.example.com/b.mp3?token=1" }, new PlaylistLoader().GetSongs(m3uFile).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_PLSWithOnlyOneStreamLink_ReturnsTheLink()
        {
            using TempFolder tempFolder = new TempFolder();
            string plsFile = WriteFile(tempFolder, "test.pls", $"[playlist]\nFile1={_StreamLink}\nTitle1=Radio\nNumberOfEntries=1\nVersion=2\n");

            CollectionAssert.AreEqual(new string[] { _StreamLink }, new PlaylistLoader().GetSongs(plsFile).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_PLSWithLocalFiles_ReturnsTheFilesInTheOrderOfTheirNumbers()
        {
            using TempFolder tempFolder = new TempFolder();
            string song1 = WriteFile(tempFolder, "testfile1.mp3", string.Empty);
            string song2 = WriteFile(tempFolder, "testfile2.mp3", string.Empty);
            string plsFile = WriteFile(tempFolder, "test.pls", "[playlist]\nFile2=./testfile1.mp3\nFile1=./testfile2.mp3\nNumberOfEntries=2\nVersion=2\n");

            CollectionAssert.AreEqual(new string[] { song2.Replace('\\', '/'), song1.Replace('\\', '/') }, new PlaylistLoader().GetSongs(plsFile).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_M3UWithRelativeFolderEntry_ReturnsTheFilesOfTheFolder()
        {
            using TempFolder tempFolder = new TempFolder();
            Directory.CreateDirectory(Path.Combine(tempFolder.Path, "Music"));
            string song1 = WriteFile(tempFolder, Path.Combine("Music", "testfile1.mp3"), string.Empty);
            string m3uFile = WriteFile(tempFolder, "test.m3u", "./Music/");

            CollectionAssert.AreEqual(new string[] { song1.Replace('\\', '/') }, new PlaylistLoader().GetSongs(m3uFile).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetSongs_FolderEntry_ReturnsOnlyFilesWithAudioFileExtensions()
        {
            using TempFolder tempFolder = new TempFolder();
            Directory.CreateDirectory(Path.Combine(tempFolder.Path, "Music"));
            string song1 = WriteFile(tempFolder, Path.Combine("Music", "testfile1.FLAC"), string.Empty);
            string song2 = WriteFile(tempFolder, Path.Combine("Music", "testfile2.wav"), string.Empty);
            WriteFile(tempFolder, Path.Combine("Music", "cover.jpg"), string.Empty);
            WriteFile(tempFolder, Path.Combine("Music", "readme.txt"), string.Empty);
            string m3uFile = WriteFile(tempFolder, "test.m3u", "./Music/");

            CollectionAssert.AreEquivalent(new string[] { song1.Replace('\\', '/'), song2.Replace('\\', '/') }, new PlaylistLoader().GetSongs(m3uFile).ToList());
        }

        private static string WriteFile(TempFolder tempFolder, string relativePath, string content)
        {
            string file = Path.Combine(tempFolder.Path, relativePath);
            GUtilities.EnsureFileExists(file);
            File.WriteAllText(file, content, new UTF8Encoding(false));
            return file;
        }
    }
}
