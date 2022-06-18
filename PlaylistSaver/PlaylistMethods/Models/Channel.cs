using Helpers;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.PlaylistMethods
{
    public class Channel
    {
        public Channel(Google.Apis.YouTube.v3.Data.Channel channelData)
        {
            // Omit the constructor when creating a object by deserialization
            if (channelData != null)
            {
                ChannelId = channelData.Id;
                Title = channelData.Snippet.Title;
                HiddenSubscriberCount = (bool)channelData.Statistics.HiddenSubscriberCount;
                if (!HiddenSubscriberCount)
                    SubscriberCount = (int)channelData.Statistics.SubscriberCount;

                // All resolutions are always available, even if user  uploads a low resolution
                // image it is just upscaled to the other ones
                // Channel thumbnails have only 3 qualities instead of 5

                // For maximum/high save high
                if ((int)Settings.ImageQuality >= 3)
                    ThumbnailURL = channelData.Snippet.Thumbnails.High.Url;
                // For low/medium save medium
                else if ((int)Settings.ImageQuality >= 1)
                    ThumbnailURL = channelData.Snippet.Thumbnails.Medium.Url;
                // For minimum save default
                else
                    ThumbnailURL = channelData.Snippet.Thumbnails.Default__.Url;
            }

            if (this.ChannelId != null)
                ChannelDirectory = GlobalItems.playlistsDirectory.SubDirectory(this.ChannelId);
        }

        public string ChannelId { get; set; }
        public string ChannelURL => $"https://www.youtube.com/channel/{ChannelId}";
        public string Title { get; set; }
        public bool HiddenSubscriberCount { get; set; }
        public int? SubscriberCount { get; set; } = null;
        public string ThumbnailURL { get; set; }

        [IgnoreDataMember]
        public string ThumbnailId => ThumbnailURL.TrimFromLast("/", false).TrimToFirst("=");
        [IgnoreDataMember]
        public string ThumbnailPath => Path.Combine(GlobalItems.channelsDirectory.FullName, $"{ChannelId}\\{ThumbnailFileName}.jpg");
        [IgnoreDataMember]
        public string ThumbnailFileName => ThumbnailURL.TrimFromLast("/", false);

        [IgnoreDataMember]
        public BitmapImage ThumbnailImage => DirectoryExtensions.GetImage(ChannelDirectory + ThumbnailFileName);

        [IgnoreDataMember]
        public DirectoryInfo ChannelDirectory { get; set; }
    }
}
