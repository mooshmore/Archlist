using Helpers;
using PlaylistSaver.Helpers;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static PlaylistSaver.PlaylistMethods.SharedClasses;

namespace PlaylistSaver.PlaylistMethods
{
    public class Playlist
    {
        public Playlist(Google.Apis.YouTube.v3.Data.Playlist apiPlaylist)
        {
            // Omit the constructor when creating a object by deserialization
            if (apiPlaylist != null)
            {
                PlaylistInfo.ETag = apiPlaylist.ETag;
                PlaylistInfo.Id = apiPlaylist.Id;
                PlaylistInfo.OwnerChannelId = apiPlaylist.Snippet.ChannelId;
                PlaylistInfo.PublishDate = (DateTime)apiPlaylist.Snippet.PublishedAt;
                PlaylistInfo.PrivacyStatus = LocalHelpers.PrivacyStatusTranslator(apiPlaylist.Status.PrivacyStatus);
                PlaylistInfo.ItemCount = (int)apiPlaylist.ContentDetails.ItemCount;

                ContentInfo.Title = apiPlaylist.Snippet.Title;
                ContentInfo.Description = apiPlaylist.Snippet.Description;

                // Check if playlist has "no thumbnail" thumbnail (when there are no videos in the playlist)
                if (apiPlaylist.Snippet.Thumbnails.Default__.Url != "https://i.ytimg.com/img/no_thumbnail.jpg")
                {
                    LocalHelpers.SaveThumbnailResolutionData(apiPlaylist.Snippet.Thumbnails, ThumbnailInfo);
                    // The default quality is always present as it is the lowest one possible so the thumbnail id can be taken from there
                    ThumbnailInfo.Id = LocalHelpers.ExtractThumbnailId(apiPlaylist.Snippet.Thumbnails.Default__.Url);
                }

            }

            PlaylistCreator = ChannelsData.GetChannel(PlaylistInfo.OwnerChannelId);
            PlaylistDirectory = Directories.PlaylistsDirectory.SubDirectory(this.PlaylistInfo.Id);
        }

        public PlaylistInfo PlaylistInfo { get; set; } = new();

        /// <summary>
        /// Information about the items content, like title, description or item count.
        /// </summary>
        public ContentInfo ContentInfo { get; set; } = new();

        /// <summary>
        /// Information about the thumbnail, like its id or maximum resolution.
        /// </summary>
        public ThumbnailInfo ThumbnailInfo { get; set; } = new();

        [IgnoreDataMember]
        public Channel PlaylistCreator { get; set; }

        [IgnoreDataMember]
        public BitmapImage ThumbnailImage => DirectoryExtensions.GetImage(PlaylistDirectory + ThumbnailInfo.FileName);

        [IgnoreDataMember]
        public DirectoryInfo PlaylistDirectory { get; set; } 
    }

    public class PlaylistInfo : ItemInfo
    {
        public int ItemCount { get; set; }
    }
}
