using GRYLibrary.Core.Misc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRYLibrary.Core.Playlists.ConcretePlaylistHandler
{
    public class M3UHandler : PlaylistFileHandler
    {
        public override void AddItemsToPlaylist(string playlistFile, IEnumerable<string> newItems)
        {
            string content = File.ReadAllText(playlistFile, this.Encoding);
            if (!content.EndsWith('\n'))
            {
                content = content + '\n';
            }
            content = content + string.Join('\n', newItems);
            File.WriteAllText(playlistFile, content, this.Encoding);
        }

        public override void CreatePlaylist(string file)
        {
            Utilities.EnsureFileExists(file);
        }

        public override void DeleteItemsFromPlaylist(string playlistFile, IEnumerable<string> itemsToDelete)
        {
            File.AppendAllLines(playlistFile, itemsToDelete.Select(song => '-' + song), this.Encoding);
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
    }
}