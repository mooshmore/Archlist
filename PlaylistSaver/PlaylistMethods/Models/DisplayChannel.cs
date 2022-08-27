﻿using Google.Apis.YouTube.v3.Data;
using Helpers;
using PlaylistSaver.Helpers;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.PlaylistMethods
{
    public class DisplayChannel
    {
        public DisplayChannel(string channelId)
        {
            var channelData = ChannelsData.ReadSavedChannelData(channelId);

            ChannelId = channelData.Id;
            ChannelTitle = channelData.Snippet.Title;

            string thumbnailPath = Path.Combine(Directories.ChannelsDirectory.FullName, ChannelId, "channelThumbnail.jpg");
            Thumbnail = new BitmapImage(new Uri(thumbnailPath));
        }

        // Web archive missing item special constructor
        public DisplayChannel(string channelId, string channelTitle)
        {
            ChannelId = channelId;
            ChannelTitle = channelTitle;

            // Use a default missing thumbnail image for web archive recovered missing items
            Thumbnail = LocalHelpers.GetResourcesBitmapImage(@"thumbnails/missingProfile.jpg");
        }


        public string ChannelId { get; }
        public string ChannelTitle { get; }
        public string ChannelUrl => "https://www.youtube.com/channel/" + ChannelId;
        public BitmapImage Thumbnail { get; }
    }
}
