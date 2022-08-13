﻿using Newtonsoft.Json;
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
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using PlaylistSaver.UserData;
using PlaylistSaver.ProgramData.Stores;
using System.Net;
using PlaylistSaver.Helpers;

namespace PlaylistSaver.PlaylistMethods
{
    public static class PlaylistsData
    {
        /// <summary>
        /// Reads all saved playlists under AppData\Roaming\MooshsPlaylistSaver\playlists, 
        /// and returns them in form of a Playlist list.
        /// </summary>
        public static List<Playlist> ReadSavedPlaylists()
        {
            List<Playlist> playlistList = new();
            foreach (DirectoryInfo playlistDirectory in Directories.PlaylistsDirectory.GetDirectories())
            {
                string playlistID = playlistDirectory.Name;
                string playlistInfoPath = Path.Combine(Directories.PlaylistsDirectory.FullName, playlistID, "playlistInfo.json");
                FileInfo playlistFile = new(playlistInfoPath);

                Playlist playlist = playlistFile.Deserialize<Playlist>();
                playlistList.Add(playlist);
            }
            return playlistList;
        }

        /// <summary>
        /// Retrieves data about all user owned playlists.
        /// </summary>
        public static async Task<PlaylistListResponse> RetrieveUserOwnedPlaylistsDataAsync()
        {
            PlaylistListResponse playlists = null;
            string nextPageToken = null;

            do
            {
                PlaylistsResource.ListRequest playlistListRequest = OAuthSystem.YoutubeService.Playlists.List(part: "contentDetails,id,snippet,status");
                playlistListRequest.Mine = true;
                if (nextPageToken != null)
                    playlistListRequest.PageToken = nextPageToken;

                PlaylistListResponse currentPlaylistListResponse = await playlistListRequest.ExecuteAsync();
                nextPageToken = currentPlaylistListResponse.NextPageToken;

                // Assign the object on the first run
                if (playlists == null)
                    playlists = currentPlaylistListResponse;
                // Merge the object on the next runs
                else
                {
                    foreach (var playlist in currentPlaylistListResponse.Items)
                    {
                        playlists.Items.Add(playlist);
                    }
                }
            } while (nextPageToken != null);

            return playlists;
        }

        /// <summary>
        /// Retrieves data about all user owned playlists.
        /// </summary>
        /// <param name="playlistIds"></param>
        /// <returns></returns>
        public static async Task<PlaylistListResponse> RetrievePlaylistsDataAsync(List<string> playlistIds)
        {
            // Just in case make so that the playlist ids won't repeat
            playlistIds = playlistIds.Distinct().ToList();

            // Contains a total list of channels in a form of a request list that are currently being retrieved
            string currentPlaylistsList = "";
            PlaylistListResponse playlists = null;

            int playlistCount = 0;
            foreach (string playlistId in playlistIds)
            {
                playlistCount++;
                currentPlaylistsList += $"{playlistId},";

                // Data of max 50 channels can be retrieved at once
                // Retrieve the data if the count has reached 50 or the item is the last retrieved item
                if (playlistCount == 50 || playlistIds.IsLastItem(playlistId))
                {
                    playlistCount = 0;
                    await GetPlaylistsDataAsync(currentPlaylistsList.TrimToLast(","));
                    currentPlaylistsList = "";
                }
            }

            // Convert the default google plyalist class to a one used in the program
            return playlists;

            // Retrieves data for the given playlists
            async Task GetPlaylistsDataAsync(string currentPlaylistsList)
            {
                PlaylistsResource.ListRequest playlistListRequest = OAuthSystem.YoutubeService.Playlists.List(part: "contentDetails,id,snippet,status");
                playlistListRequest.Id = currentPlaylistsList;


                PlaylistListResponse currentPlaylistListResponse;
                currentPlaylistListResponse = await playlistListRequest.ExecuteAsync();

                //! Handling errors kinda should be here

                // Assign the object on the first run
                if (playlists == null)
                    playlists = currentPlaylistListResponse;
                // Merge the object on the next runs
                else
                {
                    foreach (var playlist in currentPlaylistListResponse.Items)
                    {
                        playlists.Items.Add(playlist);
                    }
                }
            }
        }

        public static async Task PullPlaylistsDataAsync(List<Playlist> playlistsList)
        {
            var existingPlaylists = ReadSavedPlaylists();

            // Remove playlists from the list to not overwrite already existing playlists data
            var distinctedPlaylists = playlistsList.Where(x => !existingPlaylists.Any(y => y.Id == x.Id)).ToList();

            List<Task> thumbnailDownloads = new();

            foreach (var playlist in distinctedPlaylists)
            {
                var playlistDirectory = Directories.PlaylistsDirectory.CreateSubdirectory(playlist.Id);
                playlistDirectory.CreateSubdirectory("data");
                playlistDirectory.CreateSubdirectory("thumbnails");

                // Serialize the playlist data into a json
                string jsonString = JsonConvert.SerializeObject(playlist);
                // Create a new playlistInfo.json file and write the playlist data to it
                File.WriteAllText(playlistDirectory.CreateSubfile("playlistInfo.json").FullName, jsonString);

                // Playlists thumbnails have all resolutions, no matter the video (they are upscaled)
                // so any quality can be downloaded.

                // Note:
                // For reasons unknown to me thumbnails of resolutions: default, high and medium
                // are in proportions of 16:12 instead of 16:9 (which only medium and maxres have).
                // So, medium quality is chosen since it actualy fits required resolution and maxres 
                // would've been taking (unnecessarily) too much space.
                string thumbnailUrl = playlist.Snippet.Thumbnails.Medium.Url;
                thumbnailDownloads.Add(LocalHelpers.DownloadImageAsync(thumbnailUrl, playlistDirectory, "playlistThumbnail.jpg"));
            }

            // Download all thumbnails pararelly
            await Task.WhenAll(thumbnailDownloads);

            // Download creator channels
            await ChannelsData.PullChannelsDataAsync(distinctedPlaylists.Select(obj => obj.Snippet.ChannelId).ToList());
        }
    }
}
