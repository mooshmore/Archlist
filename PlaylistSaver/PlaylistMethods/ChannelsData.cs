using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PlaylistSaver.Windows.ViewModels;
using Google.Apis.YouTube.v3;
using Google;
using PlaylistSaver.PlaylistMethods;
using Google.Apis.YouTube.v3.Data;
using Helpers;
using System.Linq;
using PlaylistSaver.UserData;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Helpers;

namespace PlaylistSaver.PlaylistMethods
{
    public static class ChannelsData
    {
        /// <summary>
        /// Retrieves and saves playlists data and thumbnails.
        /// </summary>
        public static async Task PullChannelsDataAsync(List<string> channelIds)
        {
            //? What about updating channels data for ex. on thumbnail / channel name change?

            // Remove duplicate channel ids from the list
            List<string> channelsIdsList = channelIds.Distinct().ToList();

            // Remove already saved existing channel instances from the list
            List<string> savedChannels = Directories.ChannelsDirectory.GetSubDirectoriesNames();
            channelsIdsList = channelsIdsList.RemoveCoexistingItems(savedChannels);

            if (channelsIdsList.Count == 0)
                return;

            var retrievedChannels = await RetrieveChannelsDataAsync(channelsIdsList);

            List<Task> thumbnailDownloads = new();
            foreach (var channel in retrievedChannels.Items)
            {
                var channelDirectory = Directories.ChannelsDirectory.CreateSubdirectory(channel.Id);
                FileInfo channelData =  channelDirectory.CreateSubfile("channelInfo.json");
                channelData.Serialize(channel);

                // In channel thumbnails all qualities are always available (image is upscaled),
                // but only 3 are available - meduium, default and highres
                thumbnailDownloads.Add(LocalHelpers.DownloadImageAsync(channel.Snippet.Thumbnails.Default__.Url, channelDirectory, "channelThumbnail.jpg"));
            }
            await Task.WhenAll(thumbnailDownloads);
        }

        public static Channel ReadSavedChannelData(string channelId)
        {
            DirectoryInfo channelDirectory = Directories.ChannelsDirectory.SubDirectory(channelId);
            FileInfo channelInfo = channelDirectory.SubFile("channelInfo.json");
            return channelInfo.Deserialize<Channel>();
        }

        /// <summary>
        /// Retrieves data about channels that are in the videos in the playlist.
        /// </summary>
        public static async Task<ChannelListResponse> RetrieveChannelsDataAsync(List<string> channelsIdsList)
        {
            // Contains a total list of channels in a form of a request list that are currently being retrieved
            string currentChannelsList = "";
            ChannelListResponse channels = null;

            int channelCount = 0;
            foreach (string channelId in channelsIdsList)
            {
                channelCount++;
                currentChannelsList += $"{channelId},";

                // To not retrieve each channel one by one, they are added together to a 
                // currentChannelsList string and sent as one request;
                // There can be a data maximum of 50 channels retrieved at once, so if the
                // the channel count goes up to 50 or the channel is the last channel in the list 
                // retrieve the data then
                if (channelCount == 50 || channelsIdsList.IsLastItem(channelId))
                {
                    channelCount = 0;
                    await GetChannelsDataAsync(currentChannelsList.TrimToLast(","));
                    currentChannelsList = "";
                }
            }


            return channels;

            // Retrieves data for the given channels
            async Task GetChannelsDataAsync(string currentChannelsList)
            {
                ChannelsResource.ListRequest channelListRequest = OAuthSystem.YoutubeService.Channels.List(part: "statistics,snippet");
                channelListRequest.Id = currentChannelsList;

                ChannelListResponse currentChannelListResponse = await channelListRequest.ExecuteAsync();
                //! Some api error handling should be here

                // Assign the object on the first run
                if (channels == null)
                    channels = currentChannelListResponse;
                // Merge the object on the next runs
                else
                {
                    foreach (var channel in currentChannelListResponse.Items)
                    {
                        channels.Items.Add(channel);
                    }
                }
            }
        }
    }
}
