using Google.Apis.YouTube.v3.Data;
using Helpers;
using Archlist.Helpers;
using Archlist.ProgramData.Bases;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Archlist.PlaylistMethods.Models
{
    public class DisplayPlaylist
    {
        public DisplayPlaylist(Playlist playlist, bool isUnavailable = false)
        {
            Title = playlist.Snippet.Title;
            Description = playlist.Snippet.Description;
            Id = playlist.Id;
            ItemCount = playlist.ContentDetails.ItemCount.ToString();
            Url = "https://www.youtube.com/playlist?list=" + playlist.Id;
            PrivacyStatus = playlist.Status.PrivacyStatus.CapitalizeFirst();
            Creator = new DisplayChannel(playlist.Snippet.ChannelId);
            IsUnavailable = isUnavailable;

            if (isUnavailable)
                PlaylistDirectory = new DirectoryInfo(Path.Combine(Directories.UnavailablePlaylistsDirectory.FullName, Id));
            else
                PlaylistDirectory = new DirectoryInfo(Path.Combine(Directories.AllPlaylistsDirectory.FullName, Id));

            MissingItemsCount = GetRecentMissingItemsCount();
            DisplayMissingItems = MissingItemsCount > 0;

            // Convert to a WriteableBitmap so that the image won't be locked by a process
            ThumbnailPath = DirectoryExtensions.CreateWriteableBitmap(Path.Combine(PlaylistDirectory.FullName, "playlistThumbnail.jpg"));
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public int GetRecentMissingItemsCount() => RecentMissingItemsFile.Deserialize<List<MissingPlaylistItem>>().Count;

        public string Title { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string ItemCount { get; set; }
        public int MissingItemsCount { get; set; }
        public bool DisplayMissingItems { get; }
        public DisplayChannel Creator { get; set; }
        public bool IsUnavailable { get; }
        public string Url { get; set; }
        public string PrivacyStatus { get; private set; }
        public BitmapImage PrivacyStatusImage => PrivacyStatus switch
        {
            "Private" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/lock_64px.png"),
            "Unlisted" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/chain_64px.png"),
            "Public" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/earth_64px.png"),
            _ => null,
        };

        public WriteableBitmap ThumbnailPath { get; set; }
        public DirectoryInfo PlaylistDirectory { get; set; }

        public DirectoryInfo DataDirectory => new(Path.Combine(PlaylistDirectory.FullName, "data"));

        public FileInfo RecentMissingItemsFile => new(Path.Combine(PlaylistDirectory.FullName, "missingItems", "recent.json"));
        public FileInfo SeenMissingItemsFile => new(Path.Combine(PlaylistDirectory.FullName, Id, "missingItems", "seen.json"));
    }
}
