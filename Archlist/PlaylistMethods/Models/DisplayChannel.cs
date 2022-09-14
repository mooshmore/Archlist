using Google.Apis.YouTube.v3.Data;
using Helpers;
using Archlist.Helpers;
using Archlist.ProgramData.Stores;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace Archlist.PlaylistMethods
{
    public class DisplayChannel
    {
        public DisplayChannel(string channelId)
        {
            var channelData = ChannelsData.ReadSavedChannelData(channelId);

            ChannelId = channelData.Id;
            ChannelTitle = channelData.Snippet.Title;

            string thumbnailPath = Path.Combine(Directories.ChannelsDirectory.FullName, ChannelId, "channelThumbnail.jpg");
            Thumbnail = DirectoryExtensions.CreateWriteableBitmap(thumbnailPath);
        }

        /// <summary>
        /// Special constructor for channels recovered from a web archive that don't have thumbnails.
        /// </summary>
        public DisplayChannel(string channelId, string channelTitle)
        {
            ChannelId = channelId;
            ChannelTitle = channelTitle;

            // Use a default missing thumbnail image for web archive recovered missing items
            Thumbnail = DirectoryExtensions.CreateWriteableBitmap(@"Resources/Images/thumbnails/missingProfile.jpg");
        }


        public string ChannelId { get; }
        public string ChannelTitle { get; }
        public string ChannelUrl => "https://www.youtube.com/channel/" + ChannelId;
        public WriteableBitmap Thumbnail { get; }
    }
}
