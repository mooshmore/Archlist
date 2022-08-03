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
    public class DisplayPlaylist
    {
        public DisplayPlaylist(string playlistTitle, string playlistID, string itemCount, string creatorChannelTitle)
        {
            PlaylistTitle = playlistTitle;
            PlaylistID = playlistID;
            ItemCount = itemCount;
            CreatorChannelTitle = creatorChannelTitle;
        }

        public string PlaylistTitle { get; set; }
        public string PlaylistID { get; set; }
        public string ItemCount { get; set; }
        public string CreatorChannelTitle { get; set; }
        public string PlaylistThumbnailPath => Path.Combine(Directories.PlaylistsDirectory.FullName, PlaylistID, "playlistThumbnail.jpg");
    }


    public static class PlaylistsData
    {
        public static List<Google.Apis.YouTube.v3.Data.Playlist> ReadSavedPlaylists()
        {
            List<Google.Apis.YouTube.v3.Data.Playlist> playlistList = new();
            foreach (DirectoryInfo playlistDirectory in Directories.PlaylistsDirectory.GetDirectories())
            {
                string playlistID = playlistDirectory.Name;
                string playlistInfoPath = Path.Combine(Directories.PlaylistsDirectory.FullName, playlistID, "playlistInfo.json");
                string playlistText = File.ReadAllText(playlistInfoPath);

                Google.Apis.YouTube.v3.Data.Playlist account = JsonConvert.DeserializeObject<Google.Apis.YouTube.v3.Data.Playlist>(playlistText);
                playlistList.Add(account);
            }
            return playlistList;
        }

        public static string GetPlaylistDirectoryPath(string playlistID)
        {
            return Path.Combine(Directories.PlaylistsDirectory.FullName, playlistID);
        }

        public static List<Google.Apis.YouTube.v3.Data.Playlist> ConvertItemsToList(PlaylistListResponse playlistListResponse)
        {
            List<Google.Apis.YouTube.v3.Data.Playlist> playlistsList = new();
            foreach (var item in playlistListResponse.Items)
            {
                playlistsList.Add(item);
            }
            return playlistsList;
        }

        public static async Task<PlaylistListResponse> RetrieveUserOwnedPlaylistsData()
        {
            PlaylistListResponse playlists = null;
            string nextPageToken = null;

            do
            {
                PlaylistsResource.ListRequest playlistListRequest = OAuthLogin.youtubeService.Playlists.List(part: "contentDetails,id,snippet,status");
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
                PlaylistsResource.ListRequest playlistListRequest = OAuthLogin.youtubeService.Playlists.List(part: "contentDetails,id,snippet,status");
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

        /// <summary>
        /// Converts the default youtube api playlists object to the one used in the program.
        /// </summary>
        public static async Task<List<Playlist>> ParsePlaylists(IList<Google.Apis.YouTube.v3.Data.Playlist> youtubePlaylists)
        {
            List<Playlist> playlistsList = new();
            foreach (var item in youtubePlaylists)
            {
                playlistsList.Add(new Playlist(item));
            }
            return playlistsList;
        }

        /// <summary>
        /// Saves the playlist data locally - that includes info about the playlist in a form of a .json file and 
        /// downloading and saving the playlist thumbnail.
        /// </summary>
        /// <param name="playlistsList">The list of playlists to save information about.</param>
        public static async Task SavePlaylistData(List<Playlist> playlistsList)
        {
            // Save data for every playlist
            foreach (Playlist playlist in playlistsList)
            {
                DirectoryInfo playlistDirectory = Directories.PlaylistsDirectory.CreateSubdirectory(playlist.PlaylistInfo.Id);

                // Youtube playlist thumbnails have different url when they are changed, so only redownload
                // the thumbnail if the url(id to be precise) doesn't match or the thumbnail doesn't exist
                if (!playlistDirectory.SubfileExists_Prefix(playlist.ThumbnailInfo.Id))
                {
                    // Remove the thumbnail if the thumbnail Id doesn't match (playlistInfo is overwritten anyways)
                    playlistDirectory.DeleteAllSubfiles(".jpg");
                    // Save the thumbnail   
                    GlobalItems.WebClient.DownloadFile(playlist.ThumbnailInfo.URL, Path.Combine(playlistDirectory.FullName, playlist.ThumbnailInfo.FileName));
                }

                // Serialize the playlist data into a json
                string jsonString = JsonConvert.SerializeObject(playlist);
                // Create a new playlistInfo.json file and write the playlist data to it
                File.WriteAllText(playlistDirectory.CreateSubfile("plyalistInfo.json").FullName, jsonString);
            }
        }

        public static async Task CreatePlaylistsData(List<Google.Apis.YouTube.v3.Data.Playlist> playlistsList)
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
