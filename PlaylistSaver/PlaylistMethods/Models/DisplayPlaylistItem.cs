using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.Helpers;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class DisplayPlaylistItem : ViewModelBase
    {

        public DisplayPlaylistItem(PlaylistItem playlistItem, string playlistId)
        {
            Title = playlistItem.Snippet.Title;
            Description = playlistItem.Snippet.Description;
            PublishDate = ((DateTime)playlistItem.ContentDetails.VideoPublishedAt).ToString("dd MMM yyyy", new CultureInfo("en-GB"));
            Id = playlistItem.ContentDetails.VideoId;
            PlaylistId = playlistId;

            if (playlistItem.Snippet.Thumbnails.Medium != null)
                ThumbnailPath = new BitmapImage(new Uri(PlaylistItemsData.GetPlaylistItemThumbnailPath(PlaylistId, playlistItem.Snippet.Thumbnails.Medium.Url)));
            else
                ThumbnailPath = LocalHelpers.GetResourcesBitmapImage(@"thumbnails/missingThumbnail.jpg");
            
            Index = (playlistItem.Snippet.Position + 1).ToString();

            if (playlistItem.Snippet.VideoOwnerChannelId != null)
                Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId);
        }

        public DisplayPlaylistItem(MissingPlaylistItem playlistItem, string playlistId)
        {
            Title = playlistItem.Snippet.Title;
            Description = playlistItem.Snippet.Description;
            if (playlistItem.ContentDetails.VideoPublishedAt != null)
                PublishDate = ((DateTime)playlistItem.ContentDetails.VideoPublishedAt).ToString("dd MMM yyyy", new CultureInfo("en-GB"));
            Id = playlistItem.ContentDetails.VideoId;
            PlaylistId = playlistId;

            if (playlistItem.Snippet.Thumbnails.Medium != null)
                ThumbnailPath = new BitmapImage(new Uri(PlaylistItemsData.GetPlaylistItemThumbnailPath(PlaylistId, playlistItem.Snippet.Thumbnails.Medium.Url)));
            else
                ThumbnailPath = LocalHelpers.GetResourcesBitmapImage(@"thumbnails/missingThumbnail.jpg");

            Index = (playlistItem.Snippet.Position + 1).ToString();

            if (playlistItem.Snippet.VideoOwnerChannelId != null)
                Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId);

            RemovalYear = playlistItem.FoundMissingDate.Year.ToString();
            RemovalDayMonth = playlistItem.FoundMissingDate.ToString("dd MMMM", new CultureInfo("en-GB"));

            RemovalReasonShort = playlistItem.RemovalReasonShort;
            RemovalReasonFull = playlistItem.RemovalReasonFull;
            RemovalThumbnail = LocalHelpers.GetResourcesBitmapImage("Symbols/RemovalRed/box_important_32px.png");
        }


        public string RemovalDayMonth { get; }
        public string RemovalYear { get; }
        public string RemovalReasonShort { get; }
        public string RemovalReasonFull { get; }
        public BitmapImage RemovalThumbnail { get; }

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
        public BitmapImage ThumbnailPath { get; set; }

        public bool IsExpanded { get; } = false;
        public double DescriptionHeight { get; set; } = 0;
    }
}
