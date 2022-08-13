using Google.Apis.YouTube.v3.Data;
using Helpers;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.PlaylistMethods
{
    public class DisplayChannel
    {
        public DisplayChannel(Channel channelData)
        {
            ChannelId = channelData.Id;
            ChannelTitle = channelData.Snippet.Title;
        }

        public DisplayChannel(string channelId)
        {
            var channelData = ChannelsData.ReadSavedChannelData(channelId);

            ChannelId = channelData.Id;
            ChannelTitle = channelData.Snippet.Title;
        }

        public string ChannelId { get; set; }
        public string ChannelTitle { get; set; }
        public string ChannelUrl => "https://www.youtube.com/channel/" + ChannelId;
        public string ThumbnailPath => Path.Combine(Directories.ChannelsDirectory.FullName, ChannelId, "channelThumbnail.jpg");
    }
}
