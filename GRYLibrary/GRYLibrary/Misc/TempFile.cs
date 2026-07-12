using System;

namespace GRYLibrary.Core.Misc
{
    public sealed class TempFile : IDisposable
    {
        public string Path { get; set; }
        public TempFile() : this(Guid.NewGuid().ToString()[..8], "txt")
        {
        }
        public TempFile(string extensionWithoutDot) : this(Guid.NewGuid().ToString()[..8], extensionWithoutDot)
        {
        }
        public TempFile(string filename, string extensionWithoutDot)
        {
            this.Path = System.IO.Path.Join(System.IO.Path.GetTempPath(), filename);
            if (extensionWithoutDot != null)
            {
                this.Path = $"{this.Path}.{extensionWithoutDot}";
            }
            Utilities.EnsureFileExists(this.Path);
        }
        public void Dispose()
        {
            Utilities.EnsureFileDoesNotExist(this.Path);
        }
    }
}
