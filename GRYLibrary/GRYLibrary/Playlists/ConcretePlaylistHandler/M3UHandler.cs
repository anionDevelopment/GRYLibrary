using GRYLibrary.Core.Misc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRYLibrary.Core.Playlists.ConcretePlaylistHandler
{
    /// <summary>
    /// Handles m3u-playlist-files.
    /// </summary>
    /// <remarks>
    /// Every line which is not empty and does not start with "#" is an item; a line starting with "-" excludes the item which follows the "-".
    /// Editing never removes or changes a line, it only appends lines:
    /// Adding an item appends the item; deleting an item appends an exclusion-line ("-" followed by the item), so that also items contained in folder-entries or nested playlist-files can be deleted.
    /// Both are idempotent: a line is only appended if the playlist-file does not contain an equal line yet. Lines are equal if they are equal after removing quotes and surrounding whitespace,
    /// resolving relative items against the folder of the playlist-file and replacing "\" by "/".
    /// Because an exclusion-line excludes its item regardless of its position in the file, an item which was deleted is not contained again when it is added afterwards.
    /// Relative items are relative to the folder of the playlist-file.
    /// </remarks>
    public class M3UHandler : PlaylistFileHandler
    {
        public override void AddItemsToPlaylist(string playlistFile, IEnumerable<string> newItems)
        {
            string content = File.ReadAllText(playlistFile, this.Encoding);
            IList<string> linesToAppend = GetLinesWhichAreNotContained(content, newItems, playlistFile);
            if (linesToAppend.Count == 0)
            {
                return;
            }
            content = EnsureEndsWithLineBreak(content) + string.Join('\n', linesToAppend);
            File.WriteAllText(playlistFile, content, this.Encoding);
        }

        public override void CreatePlaylist(string file)
        {
            Utilities.EnsureFileExists(file);
        }

        public override void DeleteItemsFromPlaylist(string playlistFile, IEnumerable<string> itemsToDelete)
        {
            string content = File.ReadAllText(playlistFile, this.Encoding);
            IList<string> linesToAppend = GetLinesWhichAreNotContained(content, itemsToDelete.Select(item => $"-{item}"), playlistFile);
            if (linesToAppend.Count == 0)
            {
                return;
            }
            content = EnsureEndsWithLineBreak(content) + string.Concat(linesToAppend.Select(line => $"{line}\n"));
            File.WriteAllText(playlistFile, content, this.Encoding);
        }

        public override (ISet<string> included, ISet<string> excluded) GetItemsAndExcludedItems(string playlistFile)
        {
            IEnumerable<string> lines = File.ReadAllLines(playlistFile, this.Encoding)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
                .Select(line => line.Replace("\"", string.Empty));
            HashSet<string> includedItems = [];
            HashSet<string> excludedItems = [];
            foreach (string line in lines)
            {
                if (line.StartsWith('-'))
                {
                    excludedItems.Add(line[1..]);
                }
                else
                {
                    includedItems.Add(line);
                }
            }
            return (includedItems, excludedItems);
        }

        private static string EnsureEndsWithLineBreak(string content)
        {
            if (content.Length == 0 || content.EndsWith('\n'))
            {
                return content;
            }
            return content + '\n';
        }

        /// <summary>
        /// Returns the given lines which are neither contained in the content nor contained earlier in the given lines, in their order.
        /// </summary>
        private static IList<string> GetLinesWhichAreNotContained(string content, IEnumerable<string> lines, string playlistFile)
        {
            string folderOfPlaylistFile = Path.GetDirectoryName(Path.GetFullPath(playlistFile));
            ISet<string> containedLines = content.Split('\n')
                .Where(IsItemLine)
                .Select(line => NormalizeLineForComparison(line, folderOfPlaylistFile))
                .ToHashSet();
            List<string> result = [];
            foreach (string line in lines)
            {
                if (containedLines.Add(NormalizeLineForComparison(line, folderOfPlaylistFile)))
                {
                    result.Add(line);
                }
            }
            return result;
        }

        private static bool IsItemLine(string line)
        {
            string trimmedLine = line.Trim();
            return trimmedLine.Length != 0 && !trimmedLine.StartsWith('#');
        }

        /// <summary>
        /// Normalizes an item-line or an exclusion-line; an exclusion-line keeps its leading "-", so that it is never equal to an item-line.
        /// </summary>
        private static string NormalizeLineForComparison(string line, string folderOfPlaylistFile)
        {
            string item = line.Replace("\"", string.Empty).Trim();
            if (item.StartsWith('-'))
            {
                return "-" + NormalizeItemForComparison(item[1..], folderOfPlaylistFile);
            }
            return NormalizeItemForComparison(item, folderOfPlaylistFile);
        }

        private static string NormalizeItemForComparison(string item, string folderOfPlaylistFile)
        {
            item = item.Trim();
            if (item.StartsWith("https://") || item.StartsWith("http://"))
            {
                return item;
            }
            if (Utilities.IsRelativeLocalFilePath(item))
            {
                item = item.ResolveToFullPath(folderOfPlaylistFile);
            }
            return item.Replace('\\', '/');
        }
    }
}
