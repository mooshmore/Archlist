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
    public static class DataReassign
    {

        public static async Task ReassignData(KeyValuePair<string, PlaylistItemListResponse> playlist, List<MissingPlaylistItem> missingItems)
        {
            // Create latest playlist item data once and pass it as argument to save processing time
            var latestPlaylistItemsData = PlaylistItemsData.GetLatestPlaylistItemsData(playlist.Key);

            // Override missing item data with its old data if theres one found
            foreach (var playlistItem in missingItems)
            {
                if (ReassignData_local(latestPlaylistItemsData, playlistItem))
                    return;
                else if (await ReassignData_WebArchiveAsync(playlistItem))
                    return;
                else
                    playlistItem.RecoveryFailed = true;
            }
        }

        /// <summary>
        /// Tries to reassign data to the item by searching previously saved local data.
        /// </summary>
        /// <returns>True if the recover was succesful; False if not.</returns>
        private static bool ReassignData_local(List<PlaylistItem> latestPlaylistItemsData, MissingPlaylistItem playlistItem)
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
        /// Tries to reassign data to the item by extracting the data from the snapshots saved at http://web.archive.org/ page.
        /// </summary>
        /// <returns>True if the recover was succesful; False if not.</returns>
        public static async Task<bool> ReassignData_WebArchiveAsync(MissingPlaylistItem playlistItem)
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

                    if (await WebArchiveYoutube.ParseAsync(playlistItem, pageCode))
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

    }
}
