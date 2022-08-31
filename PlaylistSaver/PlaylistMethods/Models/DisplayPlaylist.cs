using Google.Apis.YouTube.v3.Data;
using Helpers;
using PlaylistSaver.Helpers;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class DisplayPlaylist
    {
        public DisplayPlaylist(Playlist playlist)
        {
            Title = playlist.Snippet.Title;
            Description = playlist.Snippet.Description;
            Id = playlist.Id;
            ItemCount = playlist.ContentDetails.ItemCount.ToString();
            Url = "https://www.youtube.com/playlist?list=" + playlist.Id;
            PrivacyStatus = playlist.Status.PrivacyStatus.CapitalizeFirst();

            Creator = new DisplayChannel(playlist.Snippet.ChannelId);
            MissingItemsCount = GetMissingItemsCount();

            // Convert to a WriteableBitmap so that the image won't be locked by a process
            Uri imageUri = new(Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "playlistThumbnail.jpg"));
            BitmapImage bitmapImage = new(imageUri);
            ThumbnailPath = new WriteableBitmap(bitmapImage);
        }

        public int GetMissingItemsCount()
        {
            var missingItemsFile = new FileInfo(Path.Combine(MissingItemsDirectory.FullName, "recent.json"));
            return missingItemsFile.Deserialize<List<MissingPlaylistItem>>().Count;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string ItemCount { get; set; }
        public int MissingItemsCount { get; set; }
        public DisplayChannel Creator { get; set; }

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
        public DirectoryInfo DataDirectory => new(Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "data"));
        public DirectoryInfo MissingItemsDirectory => new(Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "missingItems"));
    }
}
