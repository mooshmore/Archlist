using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Archlist.Windows.ViewModels;
using Google.Apis.YouTube.v3;
using Google;
using Archlist.PlaylistMethods;
using Google.Apis.YouTube.v3.Data;
using Helpers;
using System.Linq;
using Archlist.UserData;
using Archlist.ProgramData.Stores;
using Archlist.Helpers;
using ToastMessageService;
using System.Net;

namespace Archlist.PlaylistMethods
{
    public static class ChannelsData
    {
        /// <summary>
        /// Retrieves and saves playlists data and thumbnails.
        /// </summary>
        public static async Task PullChannelsDataAsync(Dictionary<string, PlaylistItemListResponse> playlistResponses)
        {
            ToastMessage.Loading("Downloading channels data");

            List<string> channelIds = new();
            foreach (var playlist in playlistResponses)
            {
                foreach (var item in playlist.Value.Items)
                {
                    if (item.IsAvailable())
                        channelIds.Add(item.Snippet.VideoOwnerChannelId);
                }
            }
            await PullChannelsDataAsync(channelIds);
        }

        /// <summary>
        /// Retrieves and saves playlists data and thumbnails.
        /// </summary>
        public static async Task PullChannelsDataAsync(List<string> channelIds)
        {
            // Remove duplicate channels and channels that already are saved 
            channelIds = RemoveRedundantChannels(channelIds);

            if (channelIds.Count == 0)
                return;

            // Get all channels with the given ids
            var retrievedChannels = await RetrieveChannelsDataAsync(channelIds);

            // A list that holds tasks of channel creations
            List<Task> channelCreation = new();

            foreach (var channel in retrievedChannels.Items)
            {
                // Channel data will be only created locally when all items are ready to be saved
                // to minimize the possibility of data corruption.
                channelCreation.Add(CreateChannelData(channel));

                // Downloading large quantities at once appears to throw errors, so
                // the max simultaneous count is capped at 300
                if (channelCreation.Count > 50)
                {
                    await Task.WhenAll(channelCreation);
                    channelCreation = new();
                }
            }

            // Download the rest of the channels
            await Task.WhenAll(channelCreation);

            static async Task CreateChannelData(Channel channel)
            {
                // Downloading the image data first, and only then when it is fully downloaded
                // saving it to the file
                var thumbnailData = await GlobalItems.HttpClient.DownloadBytesTaskAsync(channel.Snippet.Thumbnails.Default__.Url);

                var channelDirectory = Directories.ChannelsDirectory.CreateSubdirectory(channel.Id);
                File.WriteAllBytes(Path.Combine(channelDirectory.FullName, "channelThumbnail.jpg"), thumbnailData);
                channelDirectory.CreateSubfile("channelInfo.json").Serialize(channel);
            }
        }

        /// <summary>
        /// Removes channels that repeat in the list and also channels that already have been saved.
        /// </summary>
        private static List<string> RemoveRedundantChannels(List<string> channelIds)
        {
            // Remove channels that repeat
            channelIds.Distinct().ToList();

            // Remove already saved existing channel instances from the list
            List<string> savedChannels = Directories.ChannelsDirectory.GetSubDirectoriesNames();
            return channelIds.RemoveCoexistingItems(savedChannels);
        }


        /// <summary>
        /// Reads channel information from locally saved data.
        /// </summary>
        public static Channel ReadSavedChannelData(string channelId)
        {
            DirectoryInfo channelDirectory = Directories.ChannelsDirectory.SubDirectory(channelId);
            FileInfo channelInfo = channelDirectory.SubFile("channelInfo.json");
            return channelInfo.Deserialize<Channel>();
        }

        /// <summary>
        /// Retrieves data about channels from YoutubeAPI that are in the videos in the playlist.
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

        /// <summary>
        /// Returns current users channel.
        /// </summary>
        public static async Task<Channel> GetCurrentUserChannelAsync()
        {
            ChannelsResource.ListRequest channelListRequest = OAuthSystem.YoutubeService.Channels.List(part: "snippet");
            channelListRequest.Mine = true;
            return (await channelListRequest.ExecuteAsync()).Items[0];
        }
    }
}
