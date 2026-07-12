using System;
using System.IO;

namespace GRYLibrary.Core.Misc.FilePath
{
    public class NoPathGiven : AbstractFilePath
    {
        public override string FilePath { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override string GetPath()
        {
            return Directory.GetCurrentDirectory();
        }

        public override string GetPath(string basePath)
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
