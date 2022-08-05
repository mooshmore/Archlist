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
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using PlaylistSaver.UserData;
using PlaylistSaver.ProgramData.Stores;
using System.Net;

namespace PlaylistSaver.PlaylistMethods
{
    public static class PlaylistsData
    {
        public static List<Playlist> ReadSavedPlaylists()
        {
            List<Playlist> playlistList = new();
            foreach (DirectoryInfo playlistDirectory in Directories.PlaylistsDirectory.GetDirectories())
            {
                string playlistID = playlistDirectory.Name;
                string playlistInfoPath = Path.Combine(Directories.PlaylistsDirectory.FullName, playlistID, "playlistInfo.json");
                string playlistText = File.ReadAllText(playlistInfoPath);

                Playlist playlist = JsonConvert.DeserializeObject<Playlist>(playlistText);
                playlistList.Add(playlist);
            }
            return playlistList;
        }

        public static async Task<PlaylistListResponse> RetrieveUserOwnedPlaylistsData()
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

        public static async Task<PlaylistListResponse> RetrievePlaylistsData(List<string> playlistIds)
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
                    await GetPlaylistsData(currentPlaylistsList.TrimToLast(","));
                    currentPlaylistsList = "";
                }
            }

            // Convert the default google plyalist class to a one used in the program
            return playlists;

            // Retrieves data for the given playlists
            async Task GetPlaylistsData(string currentPlaylistsList)
            {
                PlaylistsResource.ListRequest playlistListRequest = OAuthSystem.YoutubeService.Playlists.List(part: "contentDetails,id,snippet,status");
                playlistListRequest.Id = currentPlaylistsList;


                PlaylistListResponse currentPlaylistListResponse;
                try
                {
                    currentPlaylistListResponse = await playlistListRequest.ExecuteAsync();
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

        public static async Task CreatePlaylistsData(List<Playlist> playlistsList)
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

                // Important note:
                // For reasons unknown to me thumbnails of resolutions: default, high and medium
                // are in proportions of 16:12 instead of 16:9 (which only medium and maxres have).
                // So, medium quality is chosen since it actualy fits required resolution and maxres 
                // would've been taking (unnecessarily) too much space.
                string thumbnailUrl = playlist.Snippet.Thumbnails.Medium.Url;
                string thumbnailDirectory = Path.Combine(playlistDirectory.FullName, "playlistThumbnail.jpg");

                thumbnailDownloads.Add(DownloadImage(thumbnailUrl, thumbnailDirectory));
            }

            //FileSystemWatcher();
            await Task.WhenAll(thumbnailDownloads);
        }

        public static async Task DownloadImage(string thumbnailUrl, string thumbnailDirectory)
        {
            WebClient downloadWebClient = new();
            await downloadWebClient.DownloadFileTaskAsync(new Uri(thumbnailUrl), thumbnailDirectory);
        }
    }
}
