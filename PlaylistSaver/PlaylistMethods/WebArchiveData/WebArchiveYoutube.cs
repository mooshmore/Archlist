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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playlistItem"></param>
        /// <param name="pageCode"></param>
        /// <returns>True if the parsing was succesful; False if not.</returns>
        public static async Task<bool> ParseAsync(MissingPlaylistItem playlistItem, string pageCode, string pageUrl)
        {
            // Scraping data of youtube pages is very incosistent, as there were many versions
            // of pages over the years, with differently allocated data etc. so in case
            // that parsing the data throws an exception, an information will be displayed that
            // the program was unable to parse web archive data, but the user can still see web archive
            // page by himself and check the data.

            // If the file has a record available on web archive its WebArchiveLink will be set to that link,
            // and if the parsing was succesufl SourcedFromWebArchive will be set to true.
            try
            {
                string title = pageCode.TrimFromTo("<title>", "</title>").FormatText();
                string description = pageCode.TrimFromTo("\"description\":{\"simpleText\":\"", "\"}").FormatText();
                string publishDateString = pageCode.TrimFromTo("\"dateText\":{\"simpleText\":\"", "\"}");

                // Older site versions have syntax of ex."Published on 5 May 2017",
                // instead of the new "5 may 2017", so remove it in case it appears
                if (publishDateString.Contains("Published on"))
                    publishDateString = publishDateString.TrimFrom("Published on ");

                DateTime publishDate = DateTime.ParseExact(publishDateString, new string[] { "MMM d, yyyy", "MMM dd, yyyy" }, CultureInfo.InvariantCulture);

                string author = "";
                if (pageCode.Contains("\"ownerChannelName\":\""))
                    author = pageCode.TrimFromTo("\"ownerChannelName\":\"", "\",");
                else if (pageCode.Contains("\"author\":\""))
                    author = pageCode.TrimFromTo("\"author\":\"", "\",");

                string authorId = pageCode.TrimFromTo("\"externalChannelId\":\"", "\",");


                // Assign the values only if the converting will go without errors
                playlistItem.Snippet.Title = title;
                playlistItem.Snippet.Description = description;
                playlistItem.Snippet.VideoOwnerChannelTitle = author;
                playlistItem.Snippet.VideoOwnerChannelId = authorId;
                playlistItem.ContentDetails.VideoPublishedAt = publishDate;

                playlistItem.SourcedFromWebArchive = true;
                playlistItem.WebArchiveLink = pageUrl;
                playlistItem.RecoveryFailed = false;

                return true;
            }
            catch
            {
                playlistItem.RecoveryFailed = true;
                return false;
            }
        }

        private static string FormatText(this string text)
        {
            return text.Replace().Replace(("\\n", "\n"), ( "\\r", "" ), ("&quot;", "\""));
        }
    }
}
