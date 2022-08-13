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
            Id = playlist.Id;
            ItemCount = playlist.ContentDetails.ItemCount.ToString();
            Url = "https://www.youtube.com/playlist?list=" + playlist.Id;
            PrivacyStatus = playlist.Status.PrivacyStatus.CapitalizeFirst();

            Creator = new DisplayChannel(playlist.Snippet.ChannelId);
        }

        public string Title { get; set; }
        public string Id { get; set; }
        public string ItemCount { get; set; }

        public DisplayChannel Creator { get; set; }

        public string Url { get; set; }
        public string PrivacyStatus { get; private set; }
        public BitmapImage PrivacyStatusImage => PrivacyStatus switch
        {
            "Private" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/lock_32px.png"),
            "Unlisted" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/chain_32px.png"),
            "Public" => LocalHelpers.GetResourcesBitmapImage(@"Symbols/White/earth_32px.png"),
            _ => null,
        };

        public string ThumbnailPath => Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "playlistThumbnail.jpg");

        public DirectoryInfo DataDirectory => new(Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "data"));
    }
}
