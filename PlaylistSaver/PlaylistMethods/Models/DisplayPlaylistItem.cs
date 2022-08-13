using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class DisplayPlaylistItem : ViewModelBase
    {

        public DisplayPlaylistItem(PlaylistItem playlistItem, string playlistId)
        {
            Title = playlistItem.Snippet.Title;
            Description = playlistItem.Snippet.Description;
            PublishDate = ((DateTime)playlistItem.Snippet.PublishedAt).ToString("dd MMM yyyy");
            Id = playlistItem.Id;
            PlaylistId = playlistId;
            ThumbnailPath = PlaylistItemsData.GetPlaylistItemThumbnailPath(PlaylistId, playlistItem.Snippet.Thumbnails.Medium.Url);
            Index = (playlistItem.Snippet.Position + 1).ToString();

            Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId);
        }

        public string Title { get; }

        private string _description;

        public string Description
        { 
            get => _description;
            set
            {
                RaisePropertyChanged(_description);
                _description = value;
            }
        }
        public string PublishDate { get; }
        public string Id { get; }

        public DisplayChannel Creator { get; set; }

        public string Url => "https://www.youtube.com/watch?v=" + this.Id;
        public string PlaylistId { get; }
        public string Index { get; }
        public string ThumbnailPath { get; set; }

        public bool IsExpanded { get; } = false;
        public double DescriptionHeight { get; set; } = 0;
    }
}
