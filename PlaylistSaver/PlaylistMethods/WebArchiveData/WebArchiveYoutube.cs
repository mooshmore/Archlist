using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.PlaylistMethods.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveData
{
    public static class WebArchiveYoutube
    {
        public static async Task ParseAsync(string archiveUrl, MissingPlaylistItem playlistItem)
        {
            playlistItem.WebArchiveLink = archiveUrl;
            string code = await new HttpClient().GetStringAsync(archiveUrl);

            // Scraping data of youtube pages is very incosistent, as there were many versions
            // of pages over the years, with differently allocated data etc. so in case
            // that parsing the data throws an exception, an information will be displayed that
            // the program was unable to parse web archive data, but the user can still see web archive
            // page by himself and check the data.

            // If the file has a record available on web archive its WebArchiveLink will be set to that link,
            // and if the parsing was succesufl SourcedFromWebArchive will be set to true.
            try
            {
                string title = code.TrimFromTo("<title>", "</title>");
                string description = code.TrimFromTo("\"description\":{\"simpleText\":\"", "\"}");
                string publishDateString = code.TrimFromTo("\"dateText\":{\"simpleText\":\"", "\"}");

                // Older site versions have syntax of ex."Published on 5 May 2017",
                // instead of the new "5 may 2017", so remove it in case it appears
                if (publishDateString.Contains("Published on"))
                    publishDateString = publishDateString.TrimFrom("Published on ");

                DateTime publishDate = DateTime.ParseExact(publishDateString, new string[] { "MMM d, yyyy", "MMM dd, yyyy" }, CultureInfo.InvariantCulture);

                string author = "";
                if (code.Contains("\"ownerChannelName\":\""))
                    author = code.TrimFromTo("\"ownerChannelName\":\"", "\",");
                else if (code.Contains("\"author\":\""))
                    author = code.TrimFromTo("\"author\":\"", "\",");

                string authorId = code.TrimFromTo("\"externalChannelId\":\"", "\",");


                playlistItem.Snippet.Title = title.FormatText();
                playlistItem.Snippet.Description = description.FormatText();
                playlistItem.Snippet.VideoOwnerChannelTitle = author;
                playlistItem.Snippet.VideoOwnerChannelId = authorId;
                playlistItem.ContentDetails.VideoPublishedAt = publishDate;

                playlistItem.SourcedFromWebArchive = true;
            }
            catch
            {
                return;
            }
        }

        private static string FormatText(this string text)
        {
            return text.Replace("&quot;", "\"").Replace("\\n", "\n");
        }
    }
}
