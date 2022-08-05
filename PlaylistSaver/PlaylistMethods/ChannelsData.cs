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

namespace PlaylistSaver.PlaylistMethods
{
    public static class ChannelsData
    {
        /// <summary>
        /// Returns the channel with the given Id by reading it from locally saved data.
        /// If the data doesn't exist the channel is downloaded and saved.
        /// </summary>
        public static Channel GetChannel(string channeld)
        {
            if (Directories.ChannelsDirectory.SubdirectoryExists(channeld))
            {
                DirectoryInfo channelDirectory = Directories.ChannelsDirectory.SubDirectory(channeld);
                FileInfo channelInfo = channelDirectory.SubFile("channelInfo.json");
                return JsonConvert.DeserializeObject<Channel>(channelInfo.ReadAllText());
            }
            else
            {
                List<string> channelsIdsList = new() { channeld };
                return Task.Run(async () => RetrieveAndSaveChannelsData(channelsIdsList).Result).Result[0];
            }
        }

        public static async Task<List<Channel>> RetrieveAndSaveChannelsData(List<PlaylistItem> playlistItems)
        {
            // Distinct by the channels to don't download the same channel twice
            List<string> channelsIdsList = playlistItems.Select(o => o.ItemInfo.OwnerChannelId).Distinct().ToList();

            return Task.Run(async () => RetrieveAndSaveChannelsData(channelsIdsList).Result).Result;
            // check if this works
            //return await Task.Run(() => RetrieveAndSaveChannelsData(channelsIdsList));

        }

        /// <summary>
        /// Saves the data about channels that are in the videos in the playlist.
        /// </summary>
        public static async Task<List<Channel>> RetrieveAndSaveChannelsData(List<string> channelsIdsList)
        {
            // Contains a total list of channels in a form of a request list that are currently being retrieved
            string currentChannelsList = "";
            ChannelListResponse channels = null;

            int channelCount = 0;
            foreach (string channelId in channelsIdsList)
            {
                channelCount++;
                currentChannelsList += $"{channelId},";

                // Data of max 50 channels can be retrieved at once
                // Retrieve the data if the count has reached 50 or the item is the last retrieved item
                if (channelCount == 50 || channelsIdsList.IsLastItem(channelId))
                {
                    channelCount = 0;
                    GetChannelsData(currentChannelsList.TrimToLast(",")).Wait();
                    currentChannelsList = "";
                }
            }

            List<Channel> channelsList = new();

            // Convert the default google channel class to a one used in the program
            foreach (Google.Apis.YouTube.v3.Data.Channel channelData in channels.Items)
            {
                channelsList.Add(new Channel(channelData));
            }

            // Save data for every channel
            foreach (Channel channel in channelsList)
            {
                SaveChannelData(channel);
            }

            // Retrieves data for the given channels
            async Task GetChannelsData(string currentChannelsList)
            {
                ChannelsResource.ListRequest channelListRequest = OAuthSystem.YoutubeService.Channels.List(part: "statistics,snippet");
                channelListRequest.Id = currentChannelsList;

                ChannelListResponse currentChannelListResponse;
                try
                {
                    currentChannelListResponse = await channelListRequest.ExecuteAsync();
                }
                catch (GoogleApiException requestError)
                {
                    switch (requestError.Error.Code)
                    {
                        case 404:
                            // playlist not found
                            // private playlist or incorrect link
                            //return (false, null);
                            break;
                    }
                    throw;
                }

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

            return channelsList;
        }

        /// <summary>
        /// Saves the channel data locally - that includes info about the channel in a form of a .json file and 
        /// downloading and saving the channel thumbnail.
        /// </summary>
        /// <param name="channel">The channel to save information about.</param>
        private static void SaveChannelData(Channel channel)
        {
            DirectoryInfo channelDirectory = Directories.ChannelsDirectory.CreateSubdirectory(channel.ChannelId);

            // Youtube profile thumbnails have different url when they are changed, so only redownload
            // the thumbnail if the url(id to be precise) doesn't match or the thumbnail doesn't exist
            if (!channelDirectory.SubfileExists_Prefix(channel.ThumbnailId))
            {
                // Remove the thumbnail (and all other files) if the thumbnail Id doesn't match (channelInfo is overwritten anyways)
                channelDirectory.DeleteAllSubs();
                // Save the thumbnail   
                GlobalItems.WebClient.DownloadFile(channel.ThumbnailURL, channel.ThumbnailPath);
            }

            // Serialize the channel data into a json
            string jsonString = JsonConvert.SerializeObject(channel);
            // Create a new channelInfo.json file and write the channel data to it
            File.WriteAllText(channelDirectory.CreateSubfile("channelInfo.json").FullName, jsonString);
        }

    }
}
