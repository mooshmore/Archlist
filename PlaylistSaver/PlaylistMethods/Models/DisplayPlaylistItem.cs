using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class DisplayPlaylistItem
    {
        public DisplayPlaylistItem(PlaylistItem playlistItem, string playlistId)
        {
            Title = playlistItem.Snippet.Title;
            Description = playlistItem.Snippet.Description;
            Id = playlistItem.Id;
            ThumbnailUrl = playlistItem.Snippet.Thumbnails.Medium.Url;
            PlaylistId = playlistId;

            CreatorChannelTitle = playlistItem.Snippet.ChannelTitle;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string ItemCount { get; set; }
        public string CreatorChannelTitle { get; set; }

        public string PlaylistId { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ThumbnailPath => PlaylistItemsData.GetPlaylistItemThumbnailPath(PlaylistId, ThumbnailUrl);

    }
}
