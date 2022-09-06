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

namespace Archlist.PlaylistMethods
{
    /// <summary>
    /// Methods for playlist items data that are strictly related to missing items.
    /// </summary>
    public static class MissingPlaylistItemsMethods
    {
        public static async Task PutInRemovalReason(MissingPlaylistItem playlistItem)
        {
            using (var handler = new HttpClientHandler { UseCookies = false })
            {
                var baseAddress = new Uri("https://www.youtube.com/watch?v=" + playlistItem.ContentDetails.VideoId);
                var message = new HttpRequestMessage(HttpMethod.Get, baseAddress);
                //// Always get removal reasons in english
                message.Headers.Add("Cookie", "PREF=hl=en");

                var result = await GlobalItems.HttpClient.SendAsync(message);
                var pageCode = await result.Content.ReadAsStringAsync();

                PutRemovalReason(pageCode, playlistItem);
            }

            //GlobalItems.HttpClient.Headers.Add(HttpRequestHeader.Cookie, "PREF=hl=en");
            //string htmlCode = await GlobalItems.HttpClient.DownloadStringTaskAsync("https://www.youtube.com/watch?v=" + playlistItem.ContentDetails.VideoId);
            //PutRemovalReason(htmlCode, playlistItem);
            //GlobalItems.WebClient.Headers.Remove("PREF=hl=en");
        }

        public static void PutRemovalReason(string htmlCode, MissingPlaylistItem playlistItem)
        {
            // Shorten the whole code to the part where the removal reasons are
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

        public static async Task TryToReassignDataAsync(List<PlaylistItem> latestPlaylistItemsData, MissingPlaylistItem playlistItem)
        {
            if (TryToReassignData_local(latestPlaylistItemsData, playlistItem))
                return;
            else if (await TryToReassignData_WebArchiveAsync(playlistItem))
                return;
            else
                playlistItem.RecoveryFailed = true;
        }

        /// <summary>
        /// Try to reassign data to the item by searching locally saved data.
        /// </summary>
        /// <param name="latestPlaylistItemsData"></param>
        /// <param name="playlistItem"></param>
        /// <returns></returns>
        private static bool TryToReassignData_local(List<PlaylistItem> latestPlaylistItemsData, MissingPlaylistItem playlistItem)
        {
            var foundItem = latestPlaylistItemsData.FirstOrDefault(item => item.ContentDetails.VideoId == playlistItem.ContentDetails.VideoId);
            if (foundItem == null)
                return false;
            else
            {
                foundItem.CopyProperties(playlistItem);
                playlistItem.RecoveryFailed = false;
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlistItem"></param>
        /// <returns>True if assigning data was succesful; False if not.</returns>
        public static async Task<bool> TryToReassignData_WebArchiveAsync(MissingPlaylistItem playlistItem)
        {

            string playlistItemUrl = "https://www.youtube.com/watch?v=" + playlistItem.ContentDetails.VideoId;
            var existingSnapshotsTimestamps = await WebArchiveYoutube.GetExistingSnapshots(playlistItemUrl);

            if (existingSnapshotsTimestamps.snapshotsList != null)
            {
                playlistItem.ExistingSnapshotsCount = existingSnapshotsTimestamps.maxSnapshotsCount;
                // Set a webarchive link to the first snapshot in case that all links fail at parsing
                // and don't set the link
                playlistItem.WebArchiveLink = "http://web.archive.org/web/" + existingSnapshotsTimestamps.snapshotsList[0] + "/" + playlistItemUrl;
                playlistItem.RecoveryFailed = true;

                foreach (var snapshotTimestamp in existingSnapshotsTimestamps.snapshotsList)
                {
                    string snapshotRequestUrl = "http://web.archive.org/web/" + snapshotTimestamp + "/" + playlistItemUrl;
                    Debug.WriteLine($"Attempting snapshot: {snapshotRequestUrl}");
                    // Beware that web archive responses can be awfully slow, taking even up to a minute
                    string pageCode = await GlobalItems.HttpClient.GetStringAsync(snapshotRequestUrl);

                    if (snapshotRequestUrl == "http://web.archive.org/web/20190110024731/https://www.youtube.com/watch?v=8oOF5A0wd4Q")
                    {

                    }

                    if (await WebArchiveYoutube.ParseAsync(playlistItem, pageCode, snapshotRequestUrl))
                    {
                        Debug.WriteLine("Done");

                        playlistItem.SourcedFromWebArchive = true;
                        playlistItem.WebArchiveLink = snapshotRequestUrl;
                        playlistItem.RecoveryFailed = false;

                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("---------------------------------------");
                        Debug.WriteLine("failure: " + snapshotRequestUrl);
                        Debug.WriteLine("---------------------------------------");
                    }
                }
                Debug.WriteLine("Complete parsing failure: " + playlistItemUrl);

                return false;
            }
            return false;
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
                PlaylistItemsData.SaveLatestPlaylistItemsData(playlistId, newItemsData);
            }
        }

        public static void MarkAsSeen(this DisplayPlaylist playlist) => MarkAsSeen(new List<DisplayPlaylist>() { playlist });

        public static void MarkAsSeen(List<DisplayPlaylist> playlistsList)
        {
            foreach (var playlist in playlistsList)
            {
                var recentItemsFile = new FileInfo(Path.Combine(playlist.MissingItemsDirectory.FullName, "recent.json"));
                var seenItemsFile = new FileInfo(Path.Combine(playlist.MissingItemsDirectory.FullName, "seen.json"));

                var mergedItems = recentItemsFile.Deserialize<List<MissingPlaylistItem>>();
                mergedItems.AddRange(seenItemsFile.Deserialize<List<MissingPlaylistItem>>());

                recentItemsFile.Serialize(new List<MissingPlaylistItem>());
                seenItemsFile.Serialize(mergedItems);
            }
        }

    }
}
