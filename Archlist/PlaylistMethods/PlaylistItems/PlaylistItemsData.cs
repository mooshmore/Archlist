using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google;
using Google.Apis.YouTube.v3.Data;
using Archlist.UserData;
using System;
using Archlist.ProgramData.Stores;
using System.IO;
using Utilities;
using Newtonsoft.Json;
using Archlist.Helpers;
using System.Linq;
using System.Net;
using Archlist.PlaylistMethods.Models;
using MsServices.ToastMessageService;
using System.Net.Http;
using WebArchiveData;
using System.Diagnostics;
using Archlist.PlaylistMethods.Playlists;
using Archlist.PlaylistMethods.PlaylistItems.MissingPlaylistItemsMethods;
using System.Xml;

namespace Archlist.PlaylistMethods
{
    public static class PlaylistItemsData
    {
        public static async Task PullAllPlaylistsItemsDataAsync()
        {
            await PullPlaylistsItemsDataAsync(Directories.AllPlaylistsDirectory.GetSubDirectoriesNames());
        }

        /// <summary>
        /// Retrieves and saves playlists data, updates latestPlaylistItemsData file and missing items data.
        /// </summary>
        /// <param name="playlistsIds">The playlists to retrieve data for.</param>
        public static async Task PullPlaylistsItemsDataAsync(List<string> playlistsIds)
        {
            if (!UserProfile.CheckUserProfile())
                return;

            // Check if any of playlists that were previously unavailable are available again
            var returnedPlaylists = await MissingPlaylistsData.UpdateUnavailablePlaylists();
            returnedPlaylists.ForEach(playlist => playlistsIds.Add(playlist.Id));

            // Check the playlist ids for avaialability, and split those unavailable between 
            // deleted and switched to private playlists
            var unavailablePlaylists = await MissingPlaylistsData.GetUnavailablePlaylistsAsync(playlistsIds);
            playlistsIds = unavailablePlaylists.playlistsIds;
            List<Playlist> deletedPlaylists = unavailablePlaylists.deletedPlaylists;
            List<Playlist> privatePlaylists = unavailablePlaylists.privatePlaylists;

            // Return if there are no playlists after removing unavailable playlists
            if (playlistsIds.Count == 0)
            {
                MissingPlaylistsData.HandleUnavailablePlaylistsAsync(deletedPlaylists, privatePlaylists, returnedPlaylists);
                return;
            }

            // Stores playlists and their data
            Dictionary<string, PlaylistItemListResponse> playlistResponses = await DownloadPlaylistsItemsData(playlistsIds);

            // Stores information about time when each playlist has been saved
            Dictionary<string, DateTime> playlistTimestampData = new();

            // Download creator channels
            // Creator channels are downloaded first, as playlists to work properly require channels data to exist
            await ChannelsData.PullChannelsDataAsync(playlistResponses);

            // Unable to source all playlists data
            if (playlistResponses.Count != playlistsIds.Count)
                throw new NotImplementedException();

            await DownloadPlaylistItemsThumbnails(playlistResponses);
            SavePlaylistItemsData(playlistResponses, playlistTimestampData);

            UpdateLatestPlaylistItemsData(playlistResponses);

            ToastMessage.Loading("Finishing up...");
            await UpdateMissingItemsDataAsync(playlistResponses, playlistTimestampData);
            await PlaylistsData.UpdatePlaylistsDataAsync(playlistsIds);

            MissingPlaylistsData.HandleUnavailablePlaylistsAsync(deletedPlaylists, privatePlaylists, returnedPlaylists);

            ToastMessage.Succes("All done!");
            Debug.WriteLine("All done!");
        }

        private static void SavePlaylistItemsData(Dictionary<string, PlaylistItemListResponse> playlistResponses, Dictionary<string, DateTime> playlistTimestampData)
        {
            // Save playlist items for each playlist
            foreach (var playlist in playlistResponses)
            {
                var playlistDataDirectoryPath = Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlist.Key, "data");

                DateTime currentDateTime = DateTime.Now;
                string currentDate = currentDateTime.ToString("yyyy-MM-dd");
                string currentTime = currentDateTime.ToString("HH-mm");

                playlistTimestampData.Add(playlist.Key, currentDateTime);

                DirectoryInfo playlistDataDirectory = Directory.CreateDirectory(playlistDataDirectoryPath);
                var dir = playlistDataDirectory.CreateSubdirectory(currentDate);
                var dataFile = dir.CreateSubfile($"{currentTime}.json");

                // Serialize the playlist data into a json
                string jsonString = JsonConvert.SerializeObject(playlist.Value);

                // Create a new playlistInfo.json file and write the playlist data to it
                File.WriteAllText(dataFile.FullName, jsonString);

                ToastMessage.IncrementProgress();
            }
        }

        private static async Task DownloadPlaylistItemsThumbnails(Dictionary<string, PlaylistItemListResponse> playlistResponses)
        {
            await ToastMessage.ProgressToast(playlistResponses.Count, "Downloading thumbnails for", "playlists");

            // Download playlist items thumbnails
            // Thumbnails are downloaded first before data as data requires thumbnails to work propery;
            // Thumbnails don't have any dependencies.
            foreach (var playlist in playlistResponses)
            {
                List<Task> thumbnailDownloads = new();

                foreach (var playlistItem in playlist.Value.Items)
                {
                    // Skip if the video is not available (is deleted)
                    if (!IsAvailable(playlistItem))
                        continue;

                    // Image name is its unique id in the url
                    // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
                    string imagePath = GetPlaylistItemThumbnailPath(playlist.Key, playlistItem.Snippet.Thumbnails.Medium.Url);

                    // ! Thumbnail url will have the same url even if the thumbnail has changed, so something
                    // ! like a etag should be checked if it is needed to redownload the image

                    // Don't download the image if it already exists
                    if (!File.Exists(imagePath))
                    // Medium quality is always available, no matter the uploaded image resolution,
                    // as it is upscaled to the medium quality
                    {
                        thumbnailDownloads.Add(LocalUtilities.DownloadImageAsync(playlistItem.Snippet.Thumbnails.Medium.Url, imagePath));

                        // Downloading large quantities at once appears to throw errors, so
                        // the max simultaneous count is capped at 300
                        if (thumbnailDownloads.Count > 300)
                        {
                            await Task.WhenAll(thumbnailDownloads);
                            thumbnailDownloads = new();
                        }
                    }

                }
                // Download thumbnails for each playlist separately as downloading all of them at once threw errors randomly
                // when downloading large quantities
                await Task.WhenAll(thumbnailDownloads);
                ToastMessage.IncrementProgress();
            }
        }

        private static async Task<Dictionary<string, PlaylistItemListResponse>> DownloadPlaylistsItemsData(List<string> playlistsIds)
        {
            await ToastMessage.ProgressToast(playlistsIds.Count, "Downloading data", "playlists");

            Dictionary<string, PlaylistItemListResponse> playlistResponses = new();

            //Download playlist items info for every playlist
            foreach (var playlistId in playlistsIds)
            {
                var playlistResponse = await RetrievePlaylistItemsAsync(playlistId);
                playlistResponse = await FillPlaylistItemsDurations(playlistResponse);
                playlistResponses.Add(playlistId, playlistResponse);
                ToastMessage.IncrementProgress();
            }

            return playlistResponses;
        }

        /// <summary>
        /// Retrieves information about items in the playlist from youtube with the given Id.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to retrieve.</param>
        /// <returns>The playlist in </returns>
        public static async Task<PlaylistItemListResponse> RetrievePlaylistItemsAsync(string playlistId)
        {
            PlaylistItemListResponse playlist = await GetPlaylistItemsAsync(playlistId);

            string nextPageToken = playlist.NextPageToken;
            // Only 50 items can be retrieved with a single api call per page- 
            // repeat the method until the last page is returned
            while (nextPageToken != null)
            {
                PlaylistItemListResponse nextPage = await GetPlaylistItemsAsync(playlistId, nextPageToken);
                nextPageToken = nextPage.NextPageToken;

                // Add the items from the new page to the main object
                foreach (PlaylistItem item in nextPage.Items)
                {
                    playlist.Items.Add(item);
                }
            }

            return playlist;
        }

        /// <summary>
        /// Sends a api request to retrieve the information about items in the playlist.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to retrieve.</param>
        /// <param name="nextPageToken">The next page token. If not passed the method will send a request without one.</param>
        public static async Task<PlaylistItemListResponse> GetPlaylistItemsAsync(string playlistId, string nextPageToken = null)
        {
            YouTubeService youtubeService = OAuthSystem.YoutubeService;

            PlaylistItemsResource.ListRequest request = youtubeService.PlaylistItems.List(part: "contentDetails,id,snippet,status");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;
            request.PageToken = nextPageToken;

            return await request.ExecuteAsync();
        }

        private static async Task<PlaylistItemListResponse> FillPlaylistItemsDurations(PlaylistItemListResponse playlistResponse)
        {
            List<string> videoIds = playlistResponse.Items.Select(item => item.ContentDetails.VideoId).ToList();

            string nextPageToken = null;
            VideoListResponse videosData = null;

            do
            {
                List<string> requestVideoIds = videoIds.Move(50);

                YouTubeService youtubeService = OAuthSystem.YoutubeService;
                VideosResource.ListRequest request = youtubeService.Videos.List(part: "contentDetails");
                request.Fields = "items(id,contentDetails(duration))";
                request.MaxResults = 50;
                request.Id = requestVideoIds;
                request.PageToken = nextPageToken;


                var response = await request.ExecuteAsync();
                if (videosData == null)
                    videosData = response;
                else
                    videosData.Items.AddRange(response.Items);

                nextPageToken = response.NextPageToken;

            } while (videoIds.Count != 0);

            foreach (var videoData in videosData.Items)
            {
                var matchingVideo = playlistResponse.Items.Where(items => items.ContentDetails.VideoId == videoData.Id).First();

                // The time is formatted as an ISO 8601 string.
                // Time example: PT4M13S
                // PT stands for Time Duration, 4M is 4 minutes, and 13S is 13 seconds.
                TimeSpan videoLength = XmlConvert.ToTimeSpan(videoData.ContentDetails.Duration);
                if (videoLength.IsEmpty())
                    continue;

                matchingVideo.ContentDetails.StartAt = videoLength.ToColonFormat();
            }
            return playlistResponse;
        }

        /// <summary>
        /// Puts missing items into the missing items data directories.
        /// </summary>
        private static async Task UpdateMissingItemsDataAsync(Dictionary<string, PlaylistItemListResponse> playlistResponses, Dictionary<string, DateTime> playlistTimestampData)
        {
            await ToastMessage.ProgressToast(playlistResponses.Count, "Filling missing items for", "playlists\nThis can take a long time.");
            foreach (var playlist in playlistResponses)
            {
                var allMissingRecentItems = GetAllMissingItems(playlist.Key, "recent");
                var allMissingSeenItems = GetAllMissingItems(playlist.Key, "seen");

                var missingItems = new List<MissingPlaylistItem>();
                var foundMissingDate = playlistTimestampData[playlist.Key];

                foreach (var playlistItem in playlist.Value.Items)
                {
                    // Skip if the item exists and doesn't need to be added
                    if (playlistItem.IsAvailable())
                        continue;

                    bool alreadyExistsInRecent = allMissingRecentItems.Exists(item => item.Snippet.ResourceId.VideoId == playlistItem.Snippet.ResourceId.VideoId);
                    bool alreadyExistsInSeen = allMissingSeenItems.Exists(item => item.Snippet.ResourceId.VideoId == playlistItem.Snippet.ResourceId.VideoId);

                    // Skip if the item has already been added to one of the missing lists
                    if (alreadyExistsInRecent || alreadyExistsInSeen)
                        continue;

                    var missingItem = new MissingPlaylistItem(playlistItem)
                    {
                        FoundMissingDate = foundMissingDate
                    };
                    // Add the item to save later as it is not available and hasn't been saved yet
                    missingItems.Add(missingItem);
                }

                // Fill out missing items data and save them if there are any
                if (missingItems.Count > 0)
                {
                    await DataReassign.ReassignData(playlist, missingItems);
                    await RemovalReasons.SetRemovalReasons(missingItems);

                    MissingPlaylistItemsData.SaveMissingItemsData(playlist, missingItems);
                }

                ToastMessage.IncrementProgress();
            }
        }

        private static List<MissingPlaylistItem> GetAllMissingItems(string playlistId, string itemsType)
        {
            var missingItemsDirectory = new FileInfo(Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlistId, "missingItems", $"{itemsType}.json"));
            return missingItemsDirectory.Deserialize<List<MissingPlaylistItem>>();
        }

        public static void UpdateLatestPlaylistItemsData(Dictionary<string, PlaylistItemListResponse> playlistResponses)
        {
            foreach (var (playlistId, response) in playlistResponses)
            {
                var latestItemsData = PlaylistItemsData.GetLatestPlaylistItemsData(playlistId);
                var newItemsData = new List<PlaylistItem>();

                foreach (var item in response.Items)
                {
                    // If the item has data in the response save it
                    if (item.IsAvailable())
                        newItemsData.Add(item);
                    // If not get the most recent from previous saved data (if it is available)
                    else if (latestItemsData != null)
                    {
                        var latestItemData = latestItemsData.FirstOrDefault(historyItem => item.ContentDetails.VideoId == historyItem.ContentDetails.VideoId);
                        if (latestItemData != null)
                            newItemsData.Add(latestItemData);
                    }
                }

                // Save the data
                SaveLatestPlaylistItemsData(playlistId, newItemsData);
            }
        }

        public static List<PlaylistItem> GetLatestPlaylistItemsData(string playlistId)
        {
            var latestPlaylistItemsDataFile = new FileInfo(Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlistId, "missingItems", "latestPlaylistItemsData.json"));
            return latestPlaylistItemsDataFile.Deserialize<List<PlaylistItem>>();
        }

        public static void SaveLatestPlaylistItemsData(string playlistId, List<PlaylistItem> response)
        {
            var latestPlaylistItemsDataFile = new FileInfo(Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlistId, "missingItems", "latestPlaylistItemsData.json"));
            latestPlaylistItemsDataFile.Serialize(response);
        }

        /// <summary>
        /// Checks if the given item has been deleted or changed to private.
        /// </summary>
        /// <param name="playlistItem"></param>
        /// <returns>True </returns>
        public static bool IsAvailable(this PlaylistItem playlistItem)
        {
            return playlistItem.ContentDetails.VideoPublishedAt != null;
        }

        /// <summary>
        /// Returns a thumbnail from the given playlistId directory with the given url.
        /// </summary>
        public static string GetPlaylistItemThumbnailPath(string playlistId, string thumbnailUrl, bool playlistIsUnavailable = false)
        {
            // Conversion:
            // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000/mqdefault.jpg
            string thumbnailName = thumbnailUrl.TrimFrom("vi/");

            // UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
            thumbnailName = thumbnailName.TrimTo("/") + ".jpg";

            DirectoryInfo playlistThumbnailDirectory;

            if (playlistIsUnavailable)
                playlistThumbnailDirectory = new(Path.Combine(Directories.UnavailablePlaylistsDirectory.FullName, playlistId, "thumbnails"));
            else
                playlistThumbnailDirectory = new(Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlistId, "thumbnails"));

            return Path.Combine(playlistThumbnailDirectory.FullName, thumbnailName);
        }


        public static void MarkAsSeen(this DisplayPlaylist playlist) => MarkAsSeen(new List<DisplayPlaylist>() { playlist });

        public static void MarkAsSeen(List<DisplayPlaylist> playlistsList)
        {
            foreach (var playlist in playlistsList)
            {
                var mergedItems = playlist.RecentMissingItemsFile.Deserialize<List<MissingPlaylistItem>>();
                mergedItems.AddRange(playlist.SeenMissingItemsFile.Deserialize<List<MissingPlaylistItem>>());

                playlist.RecentMissingItemsFile.Serialize(new List<MissingPlaylistItem>());
                playlist.SeenMissingItemsFile.Serialize(mergedItems);
            }
        }
    }
}
