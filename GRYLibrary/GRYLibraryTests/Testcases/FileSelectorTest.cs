using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class FileSelectorTest
    {
        [TestMethod]
        public void FileSelectorTest1()
        {
            string baseDir = "basetestdir/";
            string dir1 = baseDir + "dir1/";
            string dir2 = dir1 + "dir2/";
            string file1 = baseDir + dir1 + "file1";
            string file2 = baseDir + dir2 + "file2";
            string file3 = baseDir + dir2 + "file3";
            string file4 = baseDir + "dir3/file4";
            try
            {
                Core.Misc.Utilities.EnsureFileExists(file1, true);
                Core.Misc.Utilities.EnsureFileExists(file2, true);
                Core.Misc.Utilities.EnsureFileExists(file3, true);
                Core.Misc.Utilities.EnsureFileExists(file4, true);

                FileSelector fileSelector = FileSelector.SingleFile(file2);
                Assert.AreEqual(1, fileSelector.Files.Count());
                Assert.AreEqual(file2, fileSelector.Files.First());

                fileSelector = FileSelector.FilesInFolder(baseDir, true);
                Assert.AreEqual(4, fileSelector.Files.Count());

                fileSelector = FileSelector.FilesInFolder(baseDir, false);
                Assert.AreEqual(0, fileSelector.Files.Count());
            }
            finally
            {
                Core.Misc.Utilities.EnsureDirectoryDoesNotExist(baseDir);
            }
        }
    }
}