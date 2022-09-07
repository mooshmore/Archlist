using Google.Apis.YouTube.v3.Data;
using Helpers;
using Archlist.PlaylistMethods.Models;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.StringFunctionsExtensions;

namespace WebArchiveData
{
    public static class WebArchiveYoutube
    {
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
            else if (snapshots.Count < 50)
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
            } while (snapshots.Count < 50 && collapseFilterLevel != 16);

            List<string> snapshotTimestamps = previousSnapshots;

            // Limit to 50 results
            snapshotTimestamps.ReduceTo(50);

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
                Debug.WriteLine($"Requesting cdx: {requestString}");
                string response = await GlobalItems.HttpClient.GetStringAsync(requestString);
                Debug.WriteLine("Done");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlistItem"></param>
        /// <param name="pageCode"></param>
        /// <returns>True if the parsing was succesful; False if not.</returns>
        public static async Task<bool> ParseAsync(MissingPlaylistItem playlistItem, string pageCode)
        {
            // Scraping data of youtube pages is very incosistent, because:
            //   - there were many versions of youtube page over the years, with differently allocated fields
            //     and just overall different fields
            //   - through those different versions youtube didn't always set unique names on fields that would
            //     allow to scrape the data by a single pattern easily
            //   - the data on web archive may be incomplete / invalid
            //   - the page might be in different languages than english.
            //
            // And so that makes it impossible to just set one rule for all pages, so that's
            // why there are multiple variations of data patterns and a try catch block, as this might
            // get really messy.
            //
            // Still, if the parsing will be incorrect or just straight up won't work, the user can always
            // just check the page by himself by clicking the recovery info panel, so yeah.

            // If the file has a record available on web archive its WebArchiveLink will be set to that link,
            // and if the parsing was succesufl SourcedFromWebArchive will be set to true.
            try
            {
                DateTime publishDate = new();
                if (!ScrapeDate(pageCode, ref publishDate))
                    return false;

                string title = "";
                if (!ScrapeTitle(pageCode, ref title))
                    return false;

                string description = "";
                if (!ScrapeDescription(pageCode, ref description))
                    return false;

                string author = "";
                if (!ScrapeAuthorName(pageCode, ref author))
                    return false;

                string authorId = "";
                if (!ScrapeAuthorId(pageCode, ref authorId))
                    return false;


                // Assign the values only if the converting will go without errors
                playlistItem.Snippet.Title = title;
                playlistItem.Snippet.Description = description;
                playlistItem.Snippet.VideoOwnerChannelTitle = author;
                playlistItem.Snippet.VideoOwnerChannelId = authorId;
                playlistItem.ContentDetails.VideoPublishedAt = publishDate;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ScrapeAuthorId(string pageCode, ref string authorId)
        {
            if (pageCode.Contains("\"externalChannelId\":\""))
                authorId = pageCode.TrimFromTo("\"externalChannelId\":\"", "\",");
            else if (pageCode.Contains("\\\"externalChannelId\\\":\\\""))
                authorId = pageCode.TrimFromTo("\\\"externalChannelId\\\":\\\"", "\\\",");
            else if (pageCode.Contains("<meta itemprop=\"channelId\" content=\""))
                authorId = pageCode.TrimFromTo("<meta itemprop=\"channelId\" content=\"", "\">");
            else
                return false;

            return true;
        }

        private static bool ScrapeDate(string pageCode, ref DateTime publishDate)
        {
            string publishDateString;

            if (pageCode.Contains("\\\"dateText\\\":{\\\"simpleText\\\":\\\""))
                publishDateString = pageCode.TrimFromTo("\\\"dateText\\\":{\\\"simpleText\\\":\\\"", "\\\"}");
            else if (pageCode.Contains("\"dateText\":{\"simpleText\":\""))
                publishDateString = pageCode.TrimFromTo("\"dateText\":{\"simpleText\":\"", "\"}");
            else if (pageCode.Contains(">Published on "))
                publishDateString = pageCode.TrimFromTo(">Published on ", "<");
            else
                return false;

            // Older site versions have syntax of ex."Published on 5 May 2017",
            // instead of the new "5 may 2017", so remove it in case it appears
            //
            // Because snapshots may be in different languages, just using pattern "Published on" would be wrong,
            // so instead the text is just trimmed to the first digit which is a day
            if (publishDateString.FirstIndexOfDigit() > 7)
                publishDateString = publishDateString.Substring(publishDateString.FirstIndexOfDigit());

            // Dates in older versions in for example russian language have a "г." ending after the year,
            // like: "15 авг. 2019 г.", so this thing here is just gonna trim the text to the end of the year
            publishDateString = publishDateString.Substring(0, publishDateString.LastIndexOfDigit() + 1);

            // Dates in older versions in for example russian language have a "." ending after the month,
            // like: "15 авг. 2019", so this thing here is gonna just remove it.
            publishDateString = publishDateString.Replace(".", "");

            string[] dateFormats = new string[] 
            {   "MMM d, yyyy", "MMM dd, yyyy",
                "MMM d yyyy", "MMM dd yyyy",
                "d MMM yyyy" , "dd MMM yyyy",
                "d MMMM yyyy" , "dd MMMM yyyy"};


            // The TimeAndDate.ParseExactAnyCultureDate() has a ability to 
            // parse the date in any format by attempting to parse the text with any culture info.
            // As far as this works, its extreamely heavy on resources (it can take a few seconds
            // to execute).
            // So, first an attempt is made to parse it from english and russian (as those are the two
            // most common languages from what I've seen), and if that fails then the forbidden "parse from any"
            // method will run and lag the app for a few seconds.


            DateTime publishDateEnglish;
            DateTime publishDateRussian;
            DateTime? publishDateAny;

            // Parse english
            if (DateTime.TryParseExact(publishDateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out publishDateEnglish))
                publishDate = publishDateEnglish;
            else if (DateTime.TryParseExact(publishDateString, dateFormats, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out publishDateRussian))
            {
                publishDate = publishDateRussian;
            }
            else
            {
                publishDateAny = TimeAndDate.ParseExactAnyCultureDate(publishDateString, dateFormats);
                if (publishDateAny == null)
                    return false;
                publishDate = (DateTime)publishDateAny;
            }

            return true;
        }

        private static bool ScrapeTitle(string pageCode, ref string title)
        {
            if (pageCode.Contains("<meta name=\"title\" content=\""))
                title = pageCode.TrimFromTo("<meta name=\"title\" content=\"", "\">").FormatText();
            else if (pageCode.Contains("<title>"))
            {
                title = pageCode.TrimFromTo("<title>", "</title>").FormatText();
            }
            else
                return false;

            return true;
        }

        private static bool ScrapeDescription(string pageCode, ref string description)
        {
            if (pageCode.Contains("\\\"description\\\":{\\\"simpleText\\\":\\\""))
                description = pageCode.TrimFromTo("\\\"description\\\":{\\\"simpleText\\\":\\\"", "\\\"},").FormatText();
            else if (pageCode.Contains("\"description\":{\"simpleText\":\""))
                description = pageCode.TrimFromTo("\"description\":{\"simpleText\":\"", "\"}").FormatText();
            else if (pageCode.Contains("<meta name=\"description\" content=\""))
                description = pageCode.TrimFromTo("<meta name=\"description\" content=\"", "\">").FormatText();
            // <div id="watch-description-text" class=""> instead?
            else if (pageCode.Contains("<div id=\"watch-description-text\" class=\"\">"))
            {
                description = pageCode.TrimFromTo("<div id=\"watch-description-text\" class=\"\">", "</div>");
                description = description.Replace(
                    ("</a>", ""),
                    ("<br />", ""),
                    ("<br/>", "\n")

                    );
            }
            else
                return false;

            return true;
        }

        private static bool ScrapeAuthorName(string pageCode, ref string author)
        {
            if (pageCode.Contains("\"ownerChannelName\":\""))
                author = pageCode.TrimFromTo("\"ownerChannelName\":\"", "\",");
            else if (pageCode.Contains("\"author\":\""))
                author = pageCode.TrimFromTo("\"author\":\"", "\",");
            else if (pageCode.Contains(",\\\"author\\\":\\\""))
                author = pageCode.TrimFromTo(",\\\"author\\\":\\\"", "\\\",");
            else
                return false;

            return true;
        }

        private static string FormatText(this string text)
        {
            return text.Replace(("\\\\n", "\n"), ("\\n", "\n"), ( "\\r", "" ), ("&quot;", "\""), ("\\/", "/"), ("\\\"", "\""));
        }
    }
}
