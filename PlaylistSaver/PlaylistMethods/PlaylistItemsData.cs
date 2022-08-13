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

namespace PlaylistSaver.PlaylistMethods
{
    public static class PlaylistItemsData
    {
        /// <summary>
        /// Retrieves and saves playlists data.
        /// </summary>
        /// <param name="playlistsIds">The playlists to retrieve data for.</param>
        public static async Task PullPlaylistsItemsDataAsync(List<string> playlistsIds)
        {
            List<(string playlistId, PlaylistItemListResponse response)> playlistResponses = new();

            // Download playlist items info for every playlist
            foreach (var playlistId in playlistsIds)
            {
                playlistResponses.Add((playlistId, await RetrievePlaylistItemsAsync(playlistId)));
            }

            if (playlistResponses.Count != playlistsIds.Count)
                throw new NotImplementedException();

            // Save playlist items for each playlist
            foreach (var (playlistId, response) in playlistResponses)
            {
                var playlistDataDirectoryPath = Path.Combine(Directories.PlaylistsDirectory.FullName, playlistId, "data");

                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string currentTime = DateTime.Now.ToString("HH-mm");

                DirectoryInfo playlistDataDirectory = Directory.CreateDirectory(playlistDataDirectoryPath);
                var dir = playlistDataDirectory.CreateSubdirectory(currentDate);
                var dataFile = dir.CreateSubfile($"{currentTime}.json");

                // Serialize the playlist data into a json
                string jsonString = JsonConvert.SerializeObject(response);

                // Create a new playlistInfo.json file and write the playlist data to it
                File.WriteAllText(dataFile.FullName, jsonString);
            }

            List<Task> thumbnailDownloads = new();
            List<string> channelIds = new();

            // Download the thumbnail for each playlist item in each playlist
            // Download images pararelly to reduce the download time
            foreach (var (playlistId, response) in playlistResponses)
            {
                foreach (var playlistItem in response.Items)
                {
                    // Skip if the video is not available (is deleted)
                    if (!VideoIsAvailable(playlistItem))
                        continue;

                    // Image name is its unique id in the url
                    // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
                    string imagePath = GetPlaylistItemThumbnailPath(playlistId, playlistItem.Snippet.Thumbnails.Medium.Url);

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
        }

        public static bool VideoIsAvailable(PlaylistItem playlistItem)
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
            var result = await GetPlaylistAsync(playlistId);
            if (!result.Succes)
            { }

            PlaylistItemListResponse playlist = result.Playlist;

            string nextPageToken = playlist.NextPageToken;
            // Only 50 items can be retrieved with a single api call per page- 
            // repeat the method until the last page is returned
            while (nextPageToken != null)
            {
                result = await GetPlaylistAsync(playlistId, nextPageToken);
                if (!result.Succes)
                { }
                PlaylistItemListResponse nextPage = result.Playlist;
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
        private static async Task<(bool Succes, PlaylistItemListResponse Playlist)> GetPlaylistAsync(string playlistId, string nextPageToken = null)
        {
            YouTubeService youtubeService = OAuthSystem.YoutubeService;

            PlaylistItemsResource.ListRequest request = youtubeService.PlaylistItems.List(part: "contentDetails,id,snippet,status");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;

            if (nextPageToken != null)
                request.PageToken = nextPageToken;

            PlaylistItemListResponse response;
            try
            {
                response = await request.ExecuteAsync();
            }
            catch (GoogleApiException requestError)
            {
                switch (requestError.Error.Code)
                {
                    case 404:
                        // playlist not found
                        // private playlist or incorrect link
                        return (false, null);
                }
                throw;
            }

            return (true, response);
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
            // https://i.ytimg.com/vi/UI-GDOq8000/mqdefault.jpg => UI-GDOq8000/mqdefault.jpg
            string imageName = thumbnailUrl.TrimFromFirst("vi/");

            // UI-GDOq8000/mqdefault.jpg => UI-GDOq8000.jpg
            return imageName.TrimToFirst("/") + ".jpg";
        }
    }
}
