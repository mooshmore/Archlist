using Google.Apis.YouTube.v3.Data;
using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Archlist.Helpers;
using Utilities;
using Utilities;

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

            var thumbnailFile = new FileInfo(Path.Combine(PlaylistDirectory.FullName, "playlistThumbnail.jpg"));
            if (thumbnailFile.Exists)
                ThumbnailPath = DirectoryExtensions.CreateWriteableBitmap(thumbnailFile.FullName);
            else
                ThumbnailPath = DirectoryExtensions.CreateWriteableBitmap(@"Resources/Images/thumbnails/missingThumbnail.jpg", UriKind.Relative);

            // Convert to a WriteableBitmap so that the image won't be locked by a process
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public int GetRecentMissingItemsCount() => RecentMissingItemsFile.Deserialize<List<MissingPlaylistItem>>().Count;

        public string Title { get; set; }
        public string Description { get; }
        public string Id { get; set; }
        public string ItemCount { get; }
        public int MissingItemsCount { get; }
        public bool DisplayMissingItems { get; }
        public DisplayChannel Creator { get; }
        public bool IsUnavailable { get; }
        public string Url { get; set; }
        public string PrivacyStatus { get; private set; }
        public BitmapImage PrivacyStatusImage => PrivacyStatus switch
        {
            "Private" => LocalUtilities.GetResourcesBitmapImage(@"Symbols/White/lock_64px.png"),
            "Unlisted" => LocalUtilities.GetResourcesBitmapImage(@"Symbols/White/chain_64px.png"),
            "Public" => LocalUtilities.GetResourcesBitmapImage(@"Symbols/White/earth_64px.png"),
            _ => null,
        };

        public WriteableBitmap ThumbnailPath { get; }
        public DirectoryInfo PlaylistDirectory { get; }

        public DirectoryInfo DataDirectory => new(Path.Combine(PlaylistDirectory.FullName, "data"));

        public FileInfo RecentMissingItemsFile => new(Path.Combine(PlaylistDirectory.FullName, "missingItems", "recent.json"));
        public FileInfo SeenMissingItemsFile => new(Path.Combine(PlaylistDirectory.FullName, "missingItems", "seen.json"));
    }
}
