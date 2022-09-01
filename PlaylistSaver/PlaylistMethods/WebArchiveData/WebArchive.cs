using Google.Apis.YouTube.v3.Data;
using Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.StringFunctionsExtensions;

namespace WebArchiveData
{
    public static class WebArchive
    {
        public static async Task<string> GetLatestRecord(string url)
        {
            string response = await new HttpClient().GetStringAsync("https://archive.org/wayback/available?url=" + url);
            WebArchiveResponse webArchiveResponse = JsonConvert.DeserializeObject<WebArchiveResponse>(response);
            if (webArchiveResponse.archived_snapshots.closest == null)
                return null;
            else
                return webArchiveResponse.archived_snapshots.closest.url;
        }

        public static async Task<(List<string> snapshotsList, int maxSnapshotsCount)> GetExistingSnapshots(string url)
        {
            // What this does might be a bit confusing, but the point is it the code
            // tries to collect the most diverse collection of snapshots to 
            // increase the chances of finding one.


            // CDX server API documentation:
            // https://github.com/internetarchive/wayback/tree/master/wayback-cdx-server

            // Limit results count to a 100
            string resultLimit = "&limit=-99";

            // Only accept valid responses
            string responseCodeFilter = "&filter=statuscode:200";

            // Set the results to only show max 1 result per each month
            // (prevent cases where there are for ex. 10 snapshots in 1 day)
            string collapseFilter = "&collapse=timestamp:";
            int collapseFilterLevel = 14;

            // Only return timestamp field as it is the only one needed
            string fieldFilter = "&fl=timestamp";

            List<string> snapshots = await GetResponses();
            int maxSnapshotsCount = snapshots.Count();

            // If responses are already below 10 without any collapse limits then theres
            // no point in restricting them
            if (snapshots.Count == 0)
                return (null, 0);
            else if (snapshots.Count < 10)
            {
                return (snapshots, maxSnapshotsCount);
            }

            collapseFilterLevel = 6;
            List<string> previousSnapshots;

            // Lower the collapseFilterLevel until the number of records will go below 14
            // Start of by collapsing from the month
            do
            {
                previousSnapshots = new List<string>(snapshots);
                snapshots = await GetResponses();
                collapseFilterLevel += 2;
            } while (snapshots.Count < 10  && collapseFilterLevel != 16);

            List<string> snapshotTimestamps = previousSnapshots;

            // Limit to 15 results
            snapshotTimestamps.ReduceTo(15);

            // Also add one snapshot per for each year even further increase the chances
            // of finding the valid one
            collapseFilterLevel = 4;
            var yearResponses = await GetResponses();

            snapshotTimestamps.AddRange(yearResponses);
            snapshotTimestamps.Distinct().ToList();

            return (snapshotTimestamps, maxSnapshotsCount);

            async Task<List<string>> GetResponses()
            {
                string filters = resultLimit + fieldFilter + responseCodeFilter + collapseFilter + collapseFilterLevel;
                string requestString = "http://web.archive.org/cdx/search/cdx?url=" + url + filters;
                string response = await new HttpClient().GetStringAsync(requestString);
                if (response.IsNullOrEmpty())
                    return new List<string>();
                else
                {
                    List<string> snapshots = response.SplitBy(Delimiter.Linebreak);
                    // Remove the empty line item at the end
                    snapshots.RemoveLast();
                    // Snapshots are from oldest to newest
                    snapshots.Reverse();
                    return snapshots;
                }
            }
        }
    }
}
