using System.Collections.Generic;
using System.Threading.Tasks;
using PlaylistSaver.Windows.ViewModels;
using Google.Apis.YouTube.v3;
using Google;
using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.UserData;
using System;
using PlaylistSaver.ProgramData.Stores;
using System.IO;
using Helpers;
using Newtonsoft.Json;
using PlaylistSaver.Helpers;
using System.Linq;
using System.Net;
using PlaylistSaver.PlaylistMethods.Models;

namespace PlaylistSaver.PlaylistMethods
{
    public static class PlaylistItemsData
    {
        /// <summary>
        /// Retrieves and saves playlists data, updates latestPlaylistItemsData file and missing items data.
        /// </summary>
        /// <param name="playlistsIds">The playlists to retrieve data for.</param>
        public static async Task PullPlaylistsItemsDataAsync(List<string> playlistsIds)
        {
            Dictionary<string, PlaylistItemListResponse> playlistResponses = new();
            Dictionary<string, DateTime> playlistTimestampData = new();

            // Don't pull playlist items if they have already been pulled in the last minute
            for (int i = 0; i < playlistsIds.Count; i++)
            {
                // !uncheck this later
                //if (File.Exists(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistsIds[i], "data", DateTime.Now.ToString("yyyy-MM-dd"), $"{DateTime.Now:HH-mm}.json")))
                //{
                //    // playlist already up to date!
                //    playlistsIds.Remove(playlistsIds[i]);
                //}
            }

            if (playlistsIds.Count == 0)
                return;

            // Download playlist items info for every playlist
            foreach (var playlistId in playlistsIds)
            {
                playlistResponses.Add(playlistId, await RetrievePlaylistItemsAsync(playlistId));
            }

            // Unable to source all playlists data
            if (playlistResponses.Count != playlistsIds.Count)
                throw new NotImplementedException();

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
            }

            List<Task> thumbnailDownloads = new();
            List<string> channelIds = new();

            // Download the thumbnail for each playlist item in each playlist
            // Download images pararelly to reduce the download time
            foreach (var playlist in playlistResponses)
            {
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
                        // (though other qualities might not be available based on the uploaded images resoluion)
                        thumbnailDownloads.Add(LocalHelpers.DownloadImageAsync(playlistItem.Snippet.Thumbnails.Medium.Url, imagePath));

                    channelIds.Add(playlistItem.Snippet.VideoOwnerChannelId);
                }

            }

            // Download the images
            await Task.WhenAll(thumbnailDownloads);

            // Download creator channels
            await ChannelsData.PullChannelsDataAsync(channelIds);

            UpdateLatestPlaylistItemsData(playlistResponses);
            await UpdateMissingItemsDataAsync(playlistResponses, playlistTimestampData);
            await PlaylistsData.UpdatePlaylistsDataAsync(playlistsIds);
        }

        /// <summary>
        /// Puts missing items into the missing items data directories.
        /// </summary>
        /// <param name="playlistResponses"></param>
        private static async Task UpdateMissingItemsDataAsync(Dictionary<string, PlaylistItemListResponse> playlistResponses, Dictionary<string, DateTime> playlistTimestampData)
        {
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

                    bool alreadyExistsInRecent = allMissingRecentItems.Select(item => item.Snippet.ResourceId.VideoId).Any();
                    bool alreadyExistsInSeen = allMissingSeenItems.Select(item => item.Snippet.ResourceId.VideoId).Any();

                    // Skip if the item has already been added to one of the missing lists
                    if (alreadyExistsInRecent || alreadyExistsInSeen)
                        continue;

                    var missingItem = new MissingPlaylistItem(playlistItem);
                    missingItem.FoundMissingDate = foundMissingDate;
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
                        TryToReassignData(latestPlaylistItemsData, playlistItem);
                    }


                    List<Task> removalReasonTasks = new();
                    // Put in video removal reason
                    foreach (var playlistItem in missingItems)
                    {
                        removalReasonTasks.Add(PutInRemovalReason(playlistItem));
                    }

                    await Task.WhenAll(removalReasonTasks);

                    // Save missing items data
                    var missingItemsFile = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlist.Key, "missingItems", "recent.json"));
                    var previousMissingItems = missingItemsFile.Deserialize<List<MissingPlaylistItem>>();
                    // Merge previously missing recent items with the new ones
                    missingItems.AddRange(previousMissingItems);
                    missingItemsFile.Serialize(missingItems);
                }
            }
        }

        private static async Task PutInRemovalReason(MissingPlaylistItem playlistItem)
        {
            using WebClient client = new();
            // Always get removal reasons in english
            client.Headers.Add(HttpRequestHeader.Cookie, "PREF=hl=en");
            string htmlCode = await client.DownloadStringTaskAsync(new Uri("https://www.youtube.com/watch?v=" + playlistItem.ContentDetails.VideoId));
            PutRemovalReason(htmlCode, playlistItem);
        }

        private static void PutRemovalReason(string htmlCode, MissingPlaylistItem playlistItem)
        {
            // Shorten the whole code to the part where the removal reasons are
            string removalReasonsPart = htmlCode.TrimFromFirst("var ytInitialPlayerResponse");
            removalReasonsPart = removalReasonsPart.TrimToFirst("</script>");

            // Account closed
            if (removalReasonsPart.Contains("This video is no longer available because the uploader has closed their YouTube account."))
            {
                playlistItem.RemovalReasonShort = "Account closed";
                playlistItem.RemovalReasonFull = "Video owner has closed their Youtube account";
            }
            // Account terminated
            else if (removalReasonsPart.Contains("This video is no longer available because the YouTube account associated with this video has been terminated."))
            {
                playlistItem.RemovalReasonShort = "Account terminated";
                playlistItem.RemovalReasonFull = "Youtube account associated with this video has been terminated";
            }
            // Video private
            else if (removalReasonsPart.Contains("Sign in if you've been granted access to this video"))
            {
                playlistItem.RemovalReasonShort = "Video private";
                playlistItem.RemovalReasonFull = "This video has been set to private by the uploader";
            }
            // Video removed
            else if (removalReasonsPart.Contains("This video has been removed by the uploader"))
            {
                playlistItem.RemovalReasonShort = "Video removed";
                playlistItem.RemovalReasonFull = "This video has been removed by the uploader";
            }
            // Copyright claim
            else if (removalReasonsPart.Contains("This video is no longer available due to a copyright claim by "))
            {
                // I know I shouldn't be doing this like that but I can't rly be bothered to do it better
                string copyrightClaimer = removalReasonsPart.TrimFromFirst("claim by \"},{\"text\":\"");
                copyrightClaimer = copyrightClaimer.TrimToFirst("\"}]}");

                playlistItem.RemovalReasonShort = "Copyright claim";
                playlistItem.RemovalReasonFull = "This video is no longer available due to a copyright claim by {copyrightClaimer}";
            }
            // Guidelines strike
            else if (removalReasonsPart.Contains("This video has been removed for violating YouTube's Community Guidelines"))
            {
                playlistItem.RemovalReasonShort = "Guidelines strike";
                playlistItem.RemovalReasonFull = "This video has been removed for violating YouTube's Community Guidelines";
            }
            else
            // Unhandeled reason
            {
                playlistItem.RemovalReasonShort = "Different reason";
                playlistItem.RemovalReasonFull = "Open video in a browser to for more information";
            }
        }

        private static void TryToReassignData(List<PlaylistItem> latestPlaylistItemsData, PlaylistItem playlistItem)
        {
            if (TryToReassignData_local(latestPlaylistItemsData, playlistItem))
                return;
            else if (TryToReassignData_WebArchive(playlistItem))
                return;
            else
                AssignDefaultData(playlistItem);
        }

        private static void AssignDefaultData(PlaylistItem playlistItem)
        {
            // ? Todo
            // stuff like unknown creator etc
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Try to reassign data to the item by searching locally saved data.
        /// </summary>
        /// <param name="latestPlaylistItemsData"></param>
        /// <param name="playlistItem"></param>
        /// <returns></returns>
        private static bool TryToReassignData_local(List<PlaylistItem> latestPlaylistItemsData, PlaylistItem playlistItem)
        {
            var foundItem = latestPlaylistItemsData.FirstOrDefault(item => item.ContentDetails.VideoId == playlistItem.ContentDetails.VideoId);
            if (foundItem == null)
                return false;
            else
            {
                foundItem.CopyProperties(playlistItem);
                return true;
            }
        }

        private static bool TryToReassignData_WebArchive(PlaylistItem playlistItem)
        {
            // ? Todo
            return false;
        }


        private static List<MissingPlaylistItem> GetAllMissingItems(string playlistId, string itemsType)
        {
            var missingItemsDirectory = new FileInfo(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "missingItems", $"{itemsType}.json"));
            return missingItemsDirectory.Deserialize<List<MissingPlaylistItem>>();
        }

        private static void UpdateLatestPlaylistItemsData(Dictionary<string, PlaylistItemListResponse> playlistResponses)
        {
            foreach (var (playlistId, response) in playlistResponses)
            {
                var latestItemsData = GetLatestPlaylistItemsData(playlistId);
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
            DirectoryInfo playlistThumbnailDirectory = new(Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "thumbnails"));
            string imageName = GetPlaylistItemThumbnailName(thumbnailUrl);
            return Path.Combine(playlistThumbnailDirectory.FullName, imageName);
        }

        public static string GetPlaylistItemThumbnailName(string thumbnailUrl)
        {
            // Conversion:
            // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000/mqdefault.jpg
            string imageName = thumbnailUrl.TrimFromFirst("vi/");

            // UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
            return imageName.TrimToFirst("/") + ".jpg";
        }
    }
}
