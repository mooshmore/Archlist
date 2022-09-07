using System.Collections.Generic;
using System.Threading.Tasks;
using Archlist.Windows.ViewModels;
using Google.Apis.YouTube.v3;
using Google;
using Google.Apis.YouTube.v3.Data;
using Archlist.UserData;
using System;
using Archlist.ProgramData.Stores;
using System.IO;
using Helpers;
using Newtonsoft.Json;
using Archlist.Helpers;
using System.Linq;
using System.Net;
using Archlist.PlaylistMethods.Models;
using ToastMessageService;
using System.Net.Http;
using WebArchiveData;
using System.Diagnostics;

namespace Archlist.PlaylistMethods
{
    public static class PlaylistItemsData
    {
        public static async Task PullAllPlaylistsItemsDataAsync()
        {
            await PullPlaylistsItemsDataAsync(Directories.PlaylistsDirectory.GetSubDirectoriesNames());
        }

        /// <summary>
        /// Retrieves and saves playlists data, updates latestPlaylistItemsData file and missing items data.
        /// </summary>
        /// <param name="playlistsIds">The playlists to retrieve data for.</param>
        public static async Task PullPlaylistsItemsDataAsync(List<string> playlistsIds)
        {
            // Don't pull playlist items if they have already been pulled in the last minute
            playlistsIds = RemoveRecentlyUpdatedItems(playlistsIds);

            // Return if there are no playlists after filtering recently updated
            if (playlistsIds.Count == 0)
                return;

            // Stores playlists and their data
            Dictionary<string, PlaylistItemListResponse> playlistResponses = await DownloadPlaylistsData(playlistsIds);
            // Stores information about time when each playlist has been saved
            Dictionary<string, DateTime> playlistTimestampData = new();

            // Download creator channels
            // Creator channels are downloaded first, as playlists to work properly require channels data to exist
            await ChannelsData.PullChannelsDataAsync(playlistResponses);

            // Unable to source all playlists data
            if (playlistResponses.Count != playlistsIds.Count)
                throw new NotImplementedException();

            await DownloadPlaylistsThumbnails(playlistResponses);
            SavePlaylistsData(playlistResponses, playlistTimestampData);

            MissingPlaylistItemsMethods.UpdateLatestPlaylistItemsData(playlistResponses);

            await ToastMessage.Loading("Finishing up...");
            await UpdateMissingItemsDataAsync(playlistResponses, playlistTimestampData);
            await PlaylistsData.UpdatePlaylistsDataAsync(playlistsIds);

            await ToastMessage.Succes("All done!");
            Debug.WriteLine("All done!");
        }

        private static void SavePlaylistsData(Dictionary<string, PlaylistItemListResponse> playlistResponses, Dictionary<string, DateTime> playlistTimestampData)
        {
            // Save playlist items for each playlist
            foreach (var playlist in playlistResponses)
            {
                var playlistDataDirectoryPath = Path.Combine(Directories.PlaylistsDirectory.FullName, playlist.Key, "data");

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

        private static async Task DownloadPlaylistsThumbnails(Dictionary<string, PlaylistItemListResponse> playlistResponses)
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
                        thumbnailDownloads.Add(LocalHelpers.DownloadImageAsync(playlistItem.Snippet.Thumbnails.Medium.Url, imagePath));

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

        /// <summary>
        /// Removes
        /// </summary>
        /// <param name="playlistsIds"></param>
        /// <returns></returns>
        private static List<string> RemoveRecentlyUpdatedItems(List<string> playlistsIds)
        {
            int totalCount = playlistsIds.Count;
            for (int i = 0; i < playlistsIds.Count; i++)
            {
                //!uncheck this later
                if (File.Exists(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistsIds[i], "data", DateTime.Now.ToString("yyyy-MM-dd"), $"{DateTime.Now:HH-mm}.json")))
                {
                    // playlist already up to date!
                    playlistsIds.Remove(playlistsIds[i]);
                }
            }

            if (totalCount == 1 && playlistsIds.Count == 0)
                ToastMessage.Succes("Playlist is already up to date!");

            return playlistsIds;
        }

        private static async Task<Dictionary<string, PlaylistItemListResponse>> DownloadPlaylistsData(List<string> playlistsIds)
        {
            await ToastMessage.ProgressToast(playlistsIds.Count, "Downloading data", "playlists");

            Dictionary<string, PlaylistItemListResponse> playlistResponses = new();

            //Download playlist items info for every playlist
            foreach (var playlistId in playlistsIds)
            {
                playlistResponses.Add(playlistId, await RetrievePlaylistItemsAsync(playlistId));
                ToastMessage.IncrementProgress();
            }

            return playlistResponses;
        }

        /// <summary>
        /// Puts missing items into the missing items data directories.
        /// </summary>
        /// <param name="playlistResponses"></param>
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
                    // Create latest playlist item data once and pass it as argument to save processing time
                    var latestPlaylistItemsData = GetLatestPlaylistItemsData(playlist.Key);

                    // Override missing item data with its old data if theres one found
                    foreach (var playlistItem in missingItems)
                    {
                        await MissingPlaylistItemsMethods.TryToReassignDataAsync(latestPlaylistItemsData, playlistItem);
                    }


                    List<Task> removalReasonTasks = new();
                    // Put in video removal reason
                    foreach (var playlistItem in missingItems)
                    {
                        removalReasonTasks.Add(MissingPlaylistItemsMethods.PutInRemovalReason(playlistItem));
                    }

                    await Task.WhenAll(removalReasonTasks);

                    // Save missing items data
                    var missingItemsFile = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlist.Key, "missingItems", "recent.json"));
                    var previousMissingItems = missingItemsFile.Deserialize<List<MissingPlaylistItem>>();
                    // Merge previously missing recent items with the new ones
                    missingItems.AddRange(previousMissingItems);
                    missingItemsFile.Serialize(missingItems);
                }

                ToastMessage.IncrementProgress();
            }
        }

        private static List<MissingPlaylistItem> GetAllMissingItems(string playlistId, string itemsType)
        {
            var missingItemsDirectory = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "missingItems", $"{itemsType}.json"));
            return missingItemsDirectory.Deserialize<List<MissingPlaylistItem>>();
        }


        public static List<PlaylistItem> GetLatestPlaylistItemsData(string playlistId)
        {
            var latestPlaylistItemsDataFile = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "missingItems", "latestPlaylistItemsData.json"));
            return latestPlaylistItemsDataFile.Deserialize<List<PlaylistItem>>();
        }

        public static void SaveLatestPlaylistItemsData(string playlistId, List<PlaylistItem> response)
        {
            var latestPlaylistItemsDataFile = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "missingItems", "latestPlaylistItemsData.json"));
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
        /// Retrieves information about items in the playlist from youtube with the given Id.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to retrieve.</param>
        /// <returns>The playlist in </returns>
        public static async Task<PlaylistItemListResponse> RetrievePlaylistItemsAsync(string playlistId)
        {
            PlaylistItemListResponse playlist = await GetPlaylistAsync(playlistId);

            string nextPageToken = playlist.NextPageToken;
            // Only 50 items can be retrieved with a single api call per page- 
            // repeat the method until the last page is returned
            while (nextPageToken != null)
            {
                PlaylistItemListResponse nextPage = await GetPlaylistAsync(playlistId, nextPageToken);
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
        /// <returns>.Succes - if the request returned with a succes; .Playlist - the retrieved playlist items info (null if failed).</returns>
        private static async Task<PlaylistItemListResponse> GetPlaylistAsync(string playlistId, string nextPageToken = null)
        {
            YouTubeService youtubeService = OAuthSystem.YoutubeService;

            PlaylistItemsResource.ListRequest request = youtubeService.PlaylistItems.List(part: "contentDetails,id,snippet,status");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;

            if (nextPageToken != null)
                request.PageToken = nextPageToken;

            PlaylistItemListResponse response = await request.ExecuteAsync();
            return response;
        }

        /// <summary>
        /// Returns a thumbnail from the given playlistId directory with the given url.
        /// </summary>
        public static string GetPlaylistItemThumbnailPath(string playlistId, string thumbnailUrl)
        {
            // Conversion:
            // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000/mqdefault.jpg
            string thumbnailName = thumbnailUrl.TrimFrom("vi/");

            // UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
            thumbnailName = thumbnailName.TrimTo("/") + ".jpg";

            DirectoryInfo playlistThumbnailDirectory = new(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "thumbnails"));
            return Path.Combine(playlistThumbnailDirectory.FullName, thumbnailName);
        }
    }
}
