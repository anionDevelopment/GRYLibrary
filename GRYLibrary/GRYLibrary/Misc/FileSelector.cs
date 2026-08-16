using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.Misc
{
    /// <summary>
    /// Represens a list of files from a currently available filesystem which is genereated by a specific given function.
    /// </summary>
    public class FileSelector
    {
        public IEnumerable<string> Files { get; private set; }
        private FileSelector()
        {
        }
        public static FileSelector SingleFile(string file)
        {
            return new FileSelector
            {
                Files = new string[] { file }
            };
        }

        public static FileSelector FileList(IEnumerable<string> files)
        {
            return new FileSelector
            {
                Files = files.Select(Normalize)
            };
        }

        private static string Normalize(string file)
        {
            return file.Trim().ToLower();
        }

        public static FileSelector FilesInFolder(string folder, bool deepSearch = true)
        {
            return FilesInFolder(folder, (string file) => true, deepSearch);
        }

        public static FileSelector RegexOfFilename(string folder, string regexAsString, bool deepSearch = true)
        {
            Regex regex = new(regexAsString);
            return FilesInFolder(folder, (string file) => regex.IsMatch(Path.GetFileName(file)), deepSearch);
        }
        public static FileSelector RegexOfFileWithFullPath(string folder, string regexAsString, bool deepSearch = true)
        {
            Regex regex = new(regexAsString);
            return FilesInFolder(folder, regex.IsMatch, deepSearch);
        }
        public static FileSelector FilesInFolder(string folder, Func<string, bool> filter, bool deepSearch = true)
        {
            FileSelector result = new();
            List<string> list = [];
            if (deepSearch)
            {
                Utilities.ForEachFileAndDirectoryTransitively(folder, null, (string file, object argument) =>
                {
                    if (filter(file))
                    {
                        list.Add(file);
                    }
                }, false, null, null);
            }
            else
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    if (filter(file))
                    {
                        list.Add(Normalize(file));
                    }
                }
            }
            result.Files = list;
            return result;
        }
    }
}