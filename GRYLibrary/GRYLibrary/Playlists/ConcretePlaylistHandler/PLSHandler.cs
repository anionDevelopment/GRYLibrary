using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GRYLibrary.Core.Playlists.ConcretePlaylistHandler
{
    /// <summary>
    /// Handles pls-playlist-files.
    /// </summary>
    /// <remarks>
    /// Only reading is supported: the items are the values of the "File&lt;number&gt;"-entries, ordered by their number.
    /// pls-files do not support excluded items.
    /// </remarks>
    public class PLSHandler : PlaylistFileHandler
    {
        private static readonly Regex _FileEntryRegex = new Regex(@"^File(\d+)=(.*)$", RegexOptions.IgnoreCase);

        public override void AddItemsToPlaylist(string playlistFile, IEnumerable<string> newItems)
        {
            throw new NotImplementedException();
        }

        public override void CreatePlaylist(string file)
        {
            throw new NotImplementedException();
        }

        public override void DeleteItemsFromPlaylist(string playlistFile, IEnumerable<string> itemsToDelete)
        {
            throw new NotImplementedException();
        }

        public override (ISet<string> included, ISet<string> excluded) GetItemsAndExcludedItems(string playlistFile)
        {
            ISet<string> included = File.ReadAllLines(playlistFile, this.Encoding)
                .Select(line => _FileEntryRegex.Match(line.Trim()))
                .Where(match => match.Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
                .OrderBy(match => long.Parse(match.Groups[1].Value))
                .Select(match => match.Groups[2].Value.Trim())
                .ToHashSet();
            return (included, new HashSet<string>());
        }
    }
}
