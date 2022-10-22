using Google.Apis.YouTube.v3.Data;
using Helpers;
using Archlist.Helpers;
using Archlist.PlaylistMethods.Models;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebArchiveData;

namespace Archlist.PlaylistMethods.PlaylistItems.MissingPlaylistItemsMethods
{
    class RemovalReasons
    {
        public static async Task SetRemovalReasons(List<MissingPlaylistItem> missingItems)
        {
            List<Task> removalReasonTasks = new();
            // Sets in video removal reason
            // Youtube API doesn't provide any way to get those removal reasons,
            // so the only way to get them is to manually scrap them from the site
            foreach (var playlistItem in missingItems)
            {
                removalReasonTasks.Add(SetRemovalReason(playlistItem));
            }

            await Task.WhenAll(removalReasonTasks);
        }

        /// <summary>
        /// Sets the removal reason of the playlist item by pulling it from the video HTML page.
        /// </summary>
        public static async Task SetRemovalReason(MissingPlaylistItem playlistItem)
        {
            string pageCode = await GetVideoPageCode(playlistItem);
            ParseAndSetRemovalReason(pageCode, playlistItem);
        }

        /// <summary>
        /// Returns the HTML page of the given playlist item.
        /// </summary>
        private static async Task<string> GetVideoPageCode(MissingPlaylistItem playlistItem)
        {
            using var handler = new HttpClientHandler { UseCookies = false };
            var baseAddress = new Uri("https://www.youtube.com/watch?v=" + playlistItem.ContentDetails.VideoId);
            var message = new HttpRequestMessage(HttpMethod.Get, baseAddress);
            //// Always get removal reasons in english
            message.Headers.Add("Cookie", "PREF=hl=en");

            var result = await GlobalItems.HttpClient.SendAsync(message);
            return await result.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Determines the removal reason based on the given page code and sets it in the item.
        /// </summary>
        public static void ParseAndSetRemovalReason(string htmlCode, MissingPlaylistItem playlistItem)
        {
            // Trim the page code to the section where the removal reason is 
            string removalReasonsPart = htmlCode.TrimFrom("var ytInitialPlayerResponse");
            removalReasonsPart = removalReasonsPart.TrimTo("</script>");

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
                string copyrightClaimer = removalReasonsPart.TrimFrom("claim by \"},{\"text\":\"");
                copyrightClaimer = copyrightClaimer.TrimTo("\"}]}");

                playlistItem.RemovalReasonShort = "Copyright claim";
                playlistItem.RemovalReasonFull = $"This video is no longer available due to a copyright claim by {copyrightClaimer}";
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

    }
}
