using GRYLibrary.Core.Misc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GRYLibrary.Core.Playlists.ConcretePlaylistHandler
{
    /// <summary>
    /// Loads the songs of playlist-files (m3u and pls), including folder-entries and nested playlist-files.
    /// </summary>
    /// <remarks>
    /// Items can be local items (audio-files, folders, other playlist-files) and links ("http://..." or "https://...").
    /// Local audio-files are only loaded if their file-extension is contained in <see cref="AudioFileExtensions"/>; the content of the files is not checked.
    /// Links are always loaded; whether a link is a remote file or a stream can only be determined when it is requested.
    /// </remarks>
    public class PlaylistLoader : PlaylistFileHandler
    {
        public IDictionary<string, string> Replacements { get; set; } = new Dictionary<string, string>();
        public M3UHandler M3UHandler { get; set; } = new M3UHandler();
        public PLSHandler PLSHandler { get; set; } = new PLSHandler();
        public bool TreatEqualMetadataAsSameTrack { get; set; } = false;
        public PlaylistLoader() { }

        /// <summary>
        /// The file-extensions (lowercase, including the leading dot) of the local audio-files which are loaded; the comparison is case-insensitive.
        /// </summary>
        public static IReadOnlySet<string> AudioFileExtensions { get; } = new HashSet<string> { ".mp3", ".m4a", ".wma", ".wav", ".aac", ".flac", ".aiff", ".aif" };

        public override void AddItemsToPlaylist(string playlistFile, IEnumerable<string> newItems)
        {
            this.GetHandlerForFile(playlistFile).AddItemsToPlaylist(playlistFile, this.Replace(newItems));
        }

        public override void CreatePlaylist(string file)
        {
            this.GetHandlerForFile(file).CreatePlaylist(file);
        }

        public override void DeleteItemsFromPlaylist(string playlistFile, IEnumerable<string> itemsToDelete)
        {
            this.GetHandlerForFile(playlistFile).DeleteItemsFromPlaylist(playlistFile, this.Replace(itemsToDelete));
        }

        public override (ISet<string> included, ISet<string> excluded) GetItemsAndExcludedItems(string playlistFile)
        {
            ISet<string> notExistingItems = new HashSet<string>();
            (ISet<string> included, ISet<string> excluded) = this.GetHandlerForFile(playlistFile).GetItemsAndExcludedItems(playlistFile);
            string folderOfPlaylistFile = Path.GetDirectoryName(playlistFile);
            included = this.LoadItems(included, notExistingItems, folderOfPlaylistFile);
            excluded = this.LoadItems(excluded, notExistingItems, folderOfPlaylistFile);
            return (included, excluded);
        }
        public (ISet<string> songs, ISet<string> notExistingSongs) GetItemsAndNotExistingItems(string playlistFile)
        {
            (ISet<string> included, ISet<string> excluded) = this.GetItemsAndExcludedItems(playlistFile);
            included = included.Except(excluded).ToHashSet();
            ISet<string> existingItems = new HashSet<string>();
            ISet<string> notExistingItems = new HashSet<string>();
            foreach (string file in included)
            {
                if (this.Exists(file))
                {
                    existingItems.Add(file);
                }
                else
                {
                    notExistingItems.Add(file);
                }
            }
            if (this.TreatEqualMetadataAsSameTrack)
            {
                existingItems = this.RemoveDuplicatesByMetadataCeck(existingItems);
            }
            return (existingItems, notExistingItems);
        }

        private ISet<string> RemoveDuplicatesByMetadataCeck(ISet<string> items)
        {
            return new HashSet<string>(items.GroupBy(item =>
            {
                try
                {
                    if (this.IsLink(item))
                    {
                        return item;
                    }
                    else
                    {
                        using TagLib.File tagFile = TagLib.File.Create(item);
                        string title = tagFile.Tag.Title;
                        ISet<string> artists = tagFile.Tag.Performers.ToHashSet();
                        string artistsList = string.Join(';', artists.OrderBy(artist => $"\"{artist}\""));
                        return $"\"{title}\":{artistsList}".ToLower();
                    }
                }
                catch
                {
                    return item;
                }
            }).Select(group => group.First()));
        }

        private static bool IsAudioFile(string file)
        {
            return AudioFileExtensions.Contains(Path.GetExtension(file).ToLowerInvariant());
        }

        public override ISet<string> GetSongs(string playlistFile)
        {
            (ISet<string> songs, ISet<string> _) = this.GetItemsAndNotExistingItems(playlistFile);
            return songs;
        }
        private bool Exists(string file)
        {
            if (this.IsLink(file))
            {
                return true;//TODO
            }
            else
            {
                return File.Exists(file);
            }
        }

        private string NormalizedItem(string item, string workingDirectory)
        {
            if (item.Contains('*'))
            {
                item = item.Split('*')[0];
            }
            item = this.Replace(item);
            if (!this.IsLink(item))
            {
                bool isFolder = item.EndsWith('/') || item.EndsWith('\\');
                if (Utilities.IsRelativeLocalFilePath(item))
                {
                    item = item.ResolveToFullPath(workingDirectory);
                }
                item = item.Replace('\\', '/');
                if (isFolder && !item.EndsWith('/'))
                {
                    item = item + '/';//ResolveToFullPath removes the trailing separator, but it marks the item as folder
                }
            }
            item = item.Trim();
            return item;
        }

        /// <summary>
        /// Returns whether the item is a link to a stream (and not a local item).
        /// </summary>
        private bool IsLink(string item)
        {
            string trimmedItem = item.Trim();
            return trimmedItem.StartsWith("https://") || trimmedItem.StartsWith("http://");
        }

        private IEnumerable<string> Replace(IEnumerable<string> items)
        {
            return items.Select(this.Replace);
        }

        private string Replace(string item)
        {
            foreach (KeyValuePair<string, string> replacement in this.Replacements)
            {
                item = item.Replace(replacement.Key, replacement.Value);
            }
            return item;
        }

        private ISet<string> LoadItems(IEnumerable<string> items, ISet<string> notExistingItems, string workingDirectory)
        {
            HashSet<string> result = [];
            foreach (string item in items)
            {
                string normalizedItem = this.NormalizedItem(item, workingDirectory);
                if (this.IsLink(normalizedItem))
                {
                    result.Add(normalizedItem);
                }
                else if (normalizedItem.EndsWith('/'))
                {
                    if (Directory.Exists(normalizedItem))
                    {
                        result.UnionWith(this.LoadItems(Utilities.GetFilesOfFolderRecursively(normalizedItem), notExistingItems, workingDirectory));
                    }
                    else
                    {
                        notExistingItems.Add(normalizedItem);
                    }
                }
                else if (IsAudioFile(normalizedItem))
                {
                    result.Add(normalizedItem);
                }
                else if (this.IsSupportedPlaylistFile(normalizedItem))
                {
                    (ISet<string> songs, ISet<string> notExisting) = this.GetItemsAndNotExistingItems(normalizedItem);
                    result.UnionWith(songs);
                    notExistingItems.UnionWith(notExisting);
                }
                else
                {
                    notExistingItems.Add(item);
                }
            }
            return result;
        }
        public bool IsSupportedPlaylistFile(string item)
        {
            return item.EndsWith(".m3u") || item.EndsWith(".pls");
        }

        public PlaylistFileHandler GetHandlerForFile(string item)
        {
            if (item.EndsWith(".m3u"))
            {
                return this.M3UHandler;
            }
            if (item.EndsWith(".pls"))
            {
                return this.PLSHandler;
            }
            throw new KeyNotFoundException($"No playlisthandler available for '{item}'");
        }
    }
}