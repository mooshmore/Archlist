using Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlaylistSaver.PlaylistMethods
{
    class SavePlaylistItems
    {
        private static DirectoryInfo playlistMainDirectory;

        internal static void Save(List<PlaylistItem> playlistItems, string playlistId)
        {
            // Create directory for the playlist
            playlistMainDirectory = GlobalItems.playlistsDirectory.CreateSubdirectory(playlistId);
            DirectoryInfo dataDirectory = playlistMainDirectory.CreateSubdirectory("data");

            DirectoryInfo lastSavedDay = dataDirectory.LastCreatedDirectory();
            FileInfo lastSavedPlaylist = null;
            if (lastSavedDay != null)
                lastSavedPlaylist = lastSavedDay.LastCreatedFile();


            SavePlaylistData(playlistItems);
            RetrieveAndSaveThumbnails(playlistItems, lastSavedPlaylist);
        }

        private static void SavePlaylistData(List<PlaylistItem> playlistItems)
        {
            // Playlist data files structure:
            // playlists/{playlistId}/data/{date(yyyy-mm-dd)}/{time(hh-mm)}.json

            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");

            DirectoryInfo playlistDataDirectory = playlistMainDirectory.CreateSubdirectory("data");
            DirectoryInfo playlistDayDirectory = playlistDataDirectory.CreateSubdirectory(currentDate);

            string currentTime = DateTime.Now.ToString("HH-mm");
            string jsonString = JsonConvert.SerializeObject(playlistItems);
            File.WriteAllText(Path.Combine(playlistDayDirectory.FullName, $"{currentTime}.json"), jsonString);
        }

        /// <summary>
        /// Saves the videos thumbnails. 
        /// </summary>
        /// <remarks>
        /// There is only one thumbnail per video, so if the thumbnail of the video has changed then
        /// it is removed and a new one is downloaded.
        /// If the thumbnail hasn't changed then it isn't redownloaded.
        /// </remarks>
        /// <param name="playlistItems"></param>
        private static void RetrieveAndSaveThumbnails(List<PlaylistItem> playlistItems, FileInfo lastSavedPlaylistFile)
        {
            // Video thumbnail URL doesn't change if the thumbnail has been changed,
            // so the method checks if etag has changed to determine if it has to redownload a thumbnail
            DirectoryInfo thumbnailsDirectory = playlistMainDirectory.CreateSubdirectory("thumbnails");

            if (lastSavedPlaylistFile != null)
            {
                List<PlaylistItem> lastSavedPlaylistList = JsonConvert.DeserializeObject<List<PlaylistItem>>(lastSavedPlaylistFile.ReadAllText());

                foreach (PlaylistItem item in playlistItems)
                {
                    PlaylistItem correspondingItem = lastSavedPlaylistList.FirstOrDefault(previousItem => previousItem.ItemInfo.Id == item.ItemInfo.Id);

                    // There could be an instance where three saved files looked like this:
                    // File1: video1, video2, video3
                    // File2 (last saved): video1, video3
                    // File3 (currently saved): video1, video2, video3
                    // In that case when comparing File2 to File3, video2 wouldn't have been found - but thumbnail for it
                    // exists. Technically it can be done so that every previous file that exists should be checked but that
                    // would be very inefficient, so only the last saved file is checked.
                    //
                    // Because the thumbnail from video2 already exists, the method will delete the thumbanil with the 
                    // same id if it exists to prevent duplication of thumbnails.


                    // Download (or redownload) image if a corresponding item hasn't been found or if the video etag has changed
                    // or if it doesn't exist for some reason
                    if (correspondingItem == null || correspondingItem.ItemInfo.ETag != item.ItemInfo.ETag || !thumbnailsDirectory.SubfileExists_Prefix(item.ThumbnailInfo.Id))
                    {
                        // Delte previous thumbnail (see explanation above)

                        // Remove previous thumbnail (delete by Id prefix in case that the saved thumbnail has a different resolution than the choosen one)
                        FileInfo existingThumbnail = thumbnailsDirectory.Subfile_Prefix(item.ThumbnailInfo.Id);
                        if (existingThumbnail != null)
                            existingThumbnail.Delete();

                        string thumbnailPath = Path.Combine(thumbnailsDirectory.FullName, item.ThumbnailInfo.FileName);
                        GlobalItems.WebClient.DownloadFile(item.ThumbnailInfo.URL, thumbnailPath);
                    }
                }
            }
            // No previous playlist data found, download every video thumbnail
            else
            {
                foreach (PlaylistItem item in playlistItems)
                {
                    string thumbnailPath = Path.Combine(thumbnailsDirectory.FullName, item.ThumbnailInfo.FileName);
                    GlobalItems.WebClient.DownloadFile(item.ThumbnailInfo.URL, thumbnailPath);
                }
            }

        }
    }
}
