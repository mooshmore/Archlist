using Google.Apis.YouTube.v3.Data;
using Archlist.Helpers;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utilities;
using Utilities.WPF.Bases;

namespace Archlist.PlaylistMethods.Models
{
    public class DisplayPlaylistItem : ViewModelBase
    {
        /// <summary>
        /// Creates a display playlist item from the given missing playlist item.</br>
        /// </summary>
        /// <param name="playlistItem">The playlist item to create the object for.</param>
        /// <param name="playlistIsUnavailable">True if the playlist is unavailable; False if not.</param>
        public DisplayPlaylistItem(PlaylistItem playlistItem, bool playlistIsUnavailable)
        {
            Title = playlistItem.Snippet.Title;
            Description = playlistItem.Snippet.Description;
            if (playlistItem.ContentDetails.VideoPublishedAt != null)
                PublishDate = ((DateTime)playlistItem.ContentDetails.VideoPublishedAt).ToString("dd MMM yyyy", new CultureInfo("en-GB"));

            Id = playlistItem.ContentDetails.VideoId;
            PlaylistId = playlistItem.Snippet.PlaylistId;

            // Because the StartAt property is deprecated and no longer used,
            // the length property is stored there for the sake of simplicity
            // (the length property isn't available in the playlist item info by default)
            Length = playlistItem.ContentDetails.StartAt;
            Index = (playlistItem.Snippet.Position + 1).ToString();

            // Assign the thumbnail
            // If the thumbnail doesn't exist then just set it to a missing thumbnail image
            if (playlistItem.Snippet.Thumbnails.Medium != null)
            {
                var thumbnailFile = new FileInfo(PlaylistItemsData.GetPlaylistItemThumbnailPath(PlaylistId, playlistItem.Snippet.Thumbnails.Medium.Url, playlistIsUnavailable));
                if (thumbnailFile.Exists)
                    ThumbnailPath = new BitmapImage(new Uri(thumbnailFile.FullName));
            }
            else
                ThumbnailPath = LocalUtilities.GetResourcesBitmapImage(@"thumbnails/missingThumbnail.jpg");

            // Missing playlist item has its own creator assign.
            if (playlistItem is not MissingPlaylistItem)
                Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId);
        }

        /// <summary>
        /// Creates a display playlist item from the given missing playlist item.</br>
        /// </summary>
        /// <param name="playlistItem">The playlist item to create the object for.</param>
        /// <param name="playlistIsUnavailable">True if the playlist is unavailable; False if not.</param>
        /// <param name="isMissingItem">Used only as a signature for the missing playlist item constructor. Pass true.</param>
        public DisplayPlaylistItem(MissingPlaylistItem playlistItem, bool playlistIsUnavailable, bool isMissingItem) : this(playlistItem, playlistIsUnavailable)
        {
            // Create channel only if there is a channel Id available
            if (playlistItem.Snippet.VideoOwnerChannelId != null)
            {
                // Items with data recovered from web archive don't have thumbnails and their channel local data
                if (playlistItem.SourcedFromWebArchive)
                    Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId, playlistItem.Snippet.VideoOwnerChannelTitle);
                else
                    Creator = new DisplayChannel(playlistItem.Snippet.VideoOwnerChannelId);
            }

            RecoveryFailed = playlistItem.RecoveryFailed;
            FoundSnapshotsCount = playlistItem.ExistingSnapshotsCount;
            SourcedFromWebArchive = playlistItem.SourcedFromWebArchive;
            WebArchiveLink = playlistItem.WebArchiveLink;

            RemovalYear = playlistItem.FoundMissingDate.Year.ToString();
            RemovalDayMonth = playlistItem.FoundMissingDate.ToString("dd MMMM", new CultureInfo("en-GB"));

            RemovalReasonShort = playlistItem.RemovalReasonShort;
            RemovalReasonFull = playlistItem.RemovalReasonFull;
            RemovalThumbnail = LocalUtilities.GetResourcesBitmapImage("Symbols/RemovalRed/box_important_64px.png");
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
        public string Length { get; }
        public string Id { get; }

        public DisplayChannel Creator { get; set; }

        public string Url => "https://www.youtube.com/watch?v=" + this.Id;
        public string PlaylistId { get; }
        public string Index { get; }
        public BitmapImage ThumbnailPath { get; set; }

        // Missing items data

        public string YoutubeSearchLink => "https://www.youtube.com/results?search_query=" + System.Net.WebUtility.UrlEncode(this.Title);


        public bool RecoveryFailed { get; set; } = false;

        public bool SourcedFromWebArchive { get; set; } = false;
        public int FoundSnapshotsCount { get; set; } = 0;
        public string WebArchiveLink { get; set; }

        public string RemovalDayMonth { get; }
        public string RemovalYear { get; }
        public string RemovalReasonShort { get; }
        public string RemovalReasonFull { get; }
        public BitmapImage RemovalThumbnail { get; }
    }
}
