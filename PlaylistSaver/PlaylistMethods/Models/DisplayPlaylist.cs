using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class DisplayPlaylist
    {
        public DisplayPlaylist(Playlist playlist)
        {
            Title = playlist.Snippet.Title;
            Id = playlist.Id;
            ItemCount = playlist.ContentDetails.ItemCount.ToString();
            CreatorChannelTitle = playlist.Snippet.ChannelTitle;
        }

        public string Title { get; set; }
        public string Id { get; set; }
        public string ItemCount { get; set; }
        public string CreatorChannelTitle { get; set; }

        public string ThumbnailPath => Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "playlistThumbnail.jpg");

        public DirectoryInfo DataDirectory => new(Path.Combine(Directories.PlaylistsDirectory.FullName, Id, "data")); 
    }
}
