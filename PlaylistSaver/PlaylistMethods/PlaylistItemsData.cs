using System.Collections.Generic;
using System.Threading.Tasks;
using PlaylistSaver.Windows.ViewModels;
using Google.Apis.YouTube.v3;
using Google;
using Google.Apis.YouTube.v3.Data;

namespace PlaylistSaver.PlaylistMethods
{
    internal static class PlaylistItemsData
    {
        /// <summary>
        /// Retrieves information about items in the playlist from youtube with the given Id.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to retrieve.</param>
        /// <returns>The playlist in </returns>
        internal static async Task<List<PlaylistItem>> Retrieve(string playlistId)
        {
            var result = GetPlaylist(playlistId).Result;
            if (!result.Succes)
            { }

            PlaylistItemListResponse playlist = result.Playlist;

            string nextPageToken = playlist.NextPageToken;
            // Only 50 items can be retrieved with a single api call per page- 
            // repeat the method until the last page is returned
            while (nextPageToken != null)
            {
                result = GetPlaylist(playlistId, nextPageToken).Result;
                if (!result.Succes)
                { }
                PlaylistItemListResponse nextPage = result.Playlist;
                nextPageToken = nextPage.NextPageToken;

                // Add the items from the new page to the main object
                foreach (Google.Apis.YouTube.v3.Data.PlaylistItem item in nextPage.Items)
                {
                    playlist.Items.Add(item);
                }
            }

            // Convert default api class to the one used in the program
            return Parse(playlist);
        }

        /// <summary>
        /// Converts default api's object structure to the one used in the program.
        /// </summary>
        /// <param name="playlist">The playlist to convert.</param>
        /// <returns>A list of playlistItems converted from the given ojbect.</returns>
        internal static List<PlaylistItem> Parse(PlaylistItemListResponse playlist)
        {
            // Convert api playlistItem to a one that is used in the program
            List<PlaylistItem> playlistItems = new();

            foreach (Google.Apis.YouTube.v3.Data.PlaylistItem item in playlist.Items)
            {
                playlistItems.Add(new PlaylistItem(item));
            }
            return playlistItems;
        }

        /// <summary>
        /// Sends a api request to retrieve the information about items in the playlist.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to retrieve.</param>
        /// <param name="nextPageToken">The next page token. If not passed the method will send a request without one.</param>
        /// <returns>.Succes - if the request returned with a succes; .Playlist - the retrieved playlist items info (null if failed).</returns>
        private static async Task<(bool Succes, PlaylistItemListResponse Playlist)> GetPlaylist(string playlistId, string nextPageToken = null)
        {
            YouTubeService youtubeService = OAuthLogin.youtubeService;

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
    }
}
