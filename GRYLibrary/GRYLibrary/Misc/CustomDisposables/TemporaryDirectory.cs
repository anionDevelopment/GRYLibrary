using System;
using System.IO;

namespace GRYLibrary.Core.Misc.CustomDisposables
{
    public class TemporaryDirectory : CustomDisposable
    {
        public string TemporaryDirectoryPath { get; private set; } = null;
        public TemporaryDirectory()
        {
            this.TemporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Utilities.EnsureDirectoryExists(this.TemporaryDirectoryPath);
            this.DisposeAction = () => Utilities.EnsureDirectoryDoesNotExist(this.TemporaryDirectoryPath);
        }
    }
}