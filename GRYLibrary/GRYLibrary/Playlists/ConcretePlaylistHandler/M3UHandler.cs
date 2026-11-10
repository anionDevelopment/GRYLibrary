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
    /// Deleting an item appends an exclusion-line instead of removing lines, so that also items contained in folder-entries or nested playlist-files can be deleted.
    /// Adding an item removes the exclusion-lines of this item and appends the item, so that an item which was deleted before is contained in the playlist again.
    /// Relative items are relative to the folder of the playlist-file.
    /// </remarks>
    public class M3UHandler : PlaylistFileHandler
    {
        public override void AddItemsToPlaylist(string playlistFile, IEnumerable<string> newItems)
        {
            List<string> itemsToAdd = newItems.ToList();
            string content = File.ReadAllText(playlistFile, this.Encoding);
            content = this.RemoveExclusionLines(content, itemsToAdd, Path.GetDirectoryName(Path.GetFullPath(playlistFile)));
            content = EnsureEndsWithLineBreak(content);
            content = content + string.Join('\n', itemsToAdd);
            File.WriteAllText(playlistFile, content, this.Encoding);
        }

        public override void CreatePlaylist(string file)
        {
            Utilities.EnsureFileExists(file);
        }

        public override void DeleteItemsFromPlaylist(string playlistFile, IEnumerable<string> itemsToDelete)
        {
            string content = EnsureEndsWithLineBreak(File.ReadAllText(playlistFile, this.Encoding));
            content = content + string.Concat(itemsToDelete.Select(item => $"-{item}\n"));
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
        /// Removes the exclusion-lines which exclude one of the given items; the content is only changed if there is such a line.
        /// </summary>
        private string RemoveExclusionLines(string content, IEnumerable<string> items, string folderOfPlaylistFile)
        {
            ISet<string> normalizedItems = items.Select(item => NormalizeForComparison(item, folderOfPlaylistFile)).ToHashSet();
            string lineBreak = content.Contains("\r\n") ? "\r\n" : "\n";
            List<string> lines = content.Split(lineBreak).ToList();
            int removedLines = lines.RemoveAll(line => IsExclusionLineOf(line, normalizedItems, folderOfPlaylistFile));
            if (removedLines == 0)
            {
                return content;
            }
            return string.Join(lineBreak, lines);
        }

        private static bool IsExclusionLineOf(string line, ISet<string> normalizedItems, string folderOfPlaylistFile)
        {
            string item = line.Replace("\"", string.Empty).Trim();
            return item.StartsWith('-') && normalizedItems.Contains(NormalizeForComparison(item[1..], folderOfPlaylistFile));
        }

        private static string NormalizeForComparison(string item, string folderOfPlaylistFile)
        {
            item = item.Replace("\"", string.Empty).Trim();
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
