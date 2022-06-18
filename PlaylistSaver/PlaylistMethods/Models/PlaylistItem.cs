using PlaylistSaver.Helpers;
using System;
using static PlaylistSaver.PlaylistMethods.SharedClasses;

namespace PlaylistSaver.PlaylistMethods
{
    public class PlaylistItem
    {
        public PlaylistItem(Google.Apis.YouTube.v3.Data.PlaylistItem apiPlaylistItem)
        {
            // Omit the constructor when creating a object by deserialization
            if (apiPlaylistItem != null)
            {
                ItemInfo.ETag = apiPlaylistItem.ETag;
                ItemInfo.Id = apiPlaylistItem.Id;
                ItemInfo.OwnerChannelId = apiPlaylistItem.Snippet.VideoOwnerChannelId;
                ItemInfo.PublishDate = (DateTime)apiPlaylistItem.ContentDetails.VideoPublishedAt;
                ItemInfo.PrivacyStatus = LocalHelpers.PrivacyStatusTranslator(apiPlaylistItem.Status.PrivacyStatus);

                ItemInfo.VideoId = apiPlaylistItem.ContentDetails.VideoId;
                ItemInfo.Position = (int)apiPlaylistItem.Snippet.Position;

                ContentInfo.Title = apiPlaylistItem.Snippet.Title;
                ContentInfo.Description = apiPlaylistItem.Snippet.Description;

                LocalHelpers.SaveThumbnailResolutionData(apiPlaylistItem.Snippet.Thumbnails, ThumbnailInfo);

                // The default quality is always present as it is the lowest one possible so the thumbnail id can be taken from there
                ThumbnailInfo.Id = LocalHelpers.ExtractThumbnailId(apiPlaylistItem.Snippet.Thumbnails.Default__.Url);
            }
        }

        /// <summary>
        /// Information about the item, like its ETag or video Id.
        /// </summary>
        public PlaylistItemInfo ItemInfo { get; set; } = new PlaylistItemInfo();

        /// <summary>
        /// Information about the items content, like title or description.
        /// </summary>
        public ContentInfo ContentInfo { get; set; } = new ContentInfo();

        /// <summary>
        /// Information about the thumbnail, like its id or maximum resolution.
        /// </summary>
        public ThumbnailInfo ThumbnailInfo { get; set; } = new ThumbnailInfo();
    }

    /// <summary>
    /// Information about the item, like its ETag or video Id.
    /// </summary>
    public class PlaylistItemInfo : ItemInfo
    {
        /// <summary>
        /// The index position of the item in the playlist.
        /// </summary>
        public int Position { get; set; }

        /// <remarks>
        /// VideoId is different from Id.
        /// </remarks>
        public string VideoId { get; set; }
    }
}
