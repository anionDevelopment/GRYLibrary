using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.Playlists.ConcretePlaylistHandler
{
    public class PLSHandler : PlaylistFileHandler
    {
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
            throw new NotImplementedException();
        }
    }
}