using System;
using System.IO;

namespace GRYLibrary.Core.Misc.FilePath
{
    public class RelativeFilePath : AbstractFilePath
    {
        private string _FilePath;
        public override string FilePath
        {
            get
            {
                return this._FilePath;
            }
            set
            {
                if (!Utilities.IsRelativeLocalFilePath(value))
                {
                    throw new ArgumentException($"Expected relative path but was not a relative path: '{value}'.");
                }
                this._FilePath = value;
            }
        }
        public override string GetPath(string basePath)
        {
            return Utilities.ResolveToFullPath(this.FilePath, basePath);
        }

        public override string GetPath()
        {
            return this.GetPath(Directory.GetCurrentDirectory());
        }
    }
}
