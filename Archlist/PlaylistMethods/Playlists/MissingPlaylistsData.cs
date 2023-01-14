using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsServices.ToastMessageService;
using Utilities;

namespace Archlist.PlaylistMethods.Playlists
{
    public class MissingPlaylistsData
    {
        public static async Task<(List<string> playlistsIds, List<Playlist> deletedPlaylists, List<Playlist> privatePlaylists)> 
            GetUnavailablePlaylistsAsync(List<string> playlistsIds)
        {

            List<Playlist> deletedPlaylists = new();
            List<Playlist> privatePlaylists = new();

            List<string> nonExistentPlaylists = await FilterUnavailablePlaylists(playlistsIds);
            playlistsIds = playlistsIds.Except(nonExistentPlaylists).ToList();

            if (nonExistentPlaylists.Count > 0)
            {
                var splitPlaylists = SplitUnavailablePlaylist(nonExistentPlaylists);
                deletedPlaylists = splitPlaylists.deletedPlaylists;
                privatePlaylists = splitPlaylists.privatePlaylists;
            }

            return (playlistsIds, deletedPlaylists, privatePlaylists);
        }

        internal static async Task<List<Playlist>> UpdateUnavailablePlaylists()
        {
            List<string> unavailablePlaylistIds = Directories.UnavailablePlaylistsDirectory.GetSubDirectoriesNames();
            PlaylistListResponse existingPlaylists = await PlaylistsData.RetrievePlaylistsDataAsync(unavailablePlaylistIds);

            if (existingPlaylists == null)
                return new List<Playlist>();

            foreach (var playlist in existingPlaylists.Items)
            {
                string playlistPath = Path.Combine(Directories.UnavailablePlaylistsDirectory.FullName, playlist.Id);
                Directory.Move(playlistPath, Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlist.Id));
            }

            return existingPlaylists.Items.CreateList();
        }

        /// <summary>
        /// Returns only the playlists Ids that are unavailable from the given list of playlist Ids.
        /// </summary>
        /// <param name="playlistsIds"></param>
        /// <returns></returns>
        private static async Task<List<string>> FilterUnavailablePlaylists(List<string> playlistsIds)
        {
            var existingPlaylists = await PlaylistsData.RetrievePlaylistsDataAsync(playlistsIds);
            var allPlaylists = new List<string>(playlistsIds);

            for (int i = existingPlaylists.Items.Count - 1; i >= 0; i--)
            {
                if (allPlaylists.Contains(existingPlaylists.Items[i].Id))
                    allPlaylists.Remove(existingPlaylists.Items[i].Id);
            }

            // Only playlists that didn't have a mach are left
            return allPlaylists;
        }

        /// <summary>
        /// Moves the unavailable playlists to the "removed" folder and displays information about it to the user.
        /// </summary>
        public static void HandleUnavailablePlaylistsAsync(List<Playlist> removedPlaylists, List<Playlist> privatePlaylists, List<Playlist> returnedPlaylists)
        {
            if (removedPlaylists.Count > 0)
            {
                string removedPlaylistsInfo = "";
                foreach (var playlist in removedPlaylists)
                {
                    string playlistPath = Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlist.Id);
                    Directory.Move(playlistPath, Path.Combine(Directories.UnavailablePlaylistsDirectory.FullName, playlist.Id));
                    removedPlaylistsInfo += playlist.Snippet.Localized.Title + ", ";
                }

                if (removedPlaylistsInfo != "")
                    ToastMessage.InformationDialog("Done\n\nFollowing playlists have been found removed from Youtube or set to private:\n" + removedPlaylistsInfo.TrimToLast(", "));
            }

            if (privatePlaylists.Count > 0)
            {
                string privatePlaylistsInfo = "";
                List<string> privatePlaylistsChannels = privatePlaylists.Select(playlist => playlist.Snippet.ChannelTitle).Distinct().ToList();

                foreach (var channel in privatePlaylistsChannels)
                {
                    privatePlaylistsInfo += channel + ", ";
                }

                if (privatePlaylistsInfo != "")
                    ToastMessage.InformationDialog($"Done!\n\n{privatePlaylists.Count} playlists from your other accounts weren't updates because they are set as private.\n" + "Accounts: " + privatePlaylistsInfo.TrimToLast(", ") + "';'Switch to those accounts to update them or set the playlists as public/unlisted\nto allow updating them from any account.");
            }

            if (returnedPlaylists.Count > 0)
            {
                string returnedPlaylistsInfo = "";
                foreach (var playlist in returnedPlaylists)
                {
                    returnedPlaylistsInfo += playlist.Snippet.Localized.Title + $" ({playlist.Snippet.ChannelTitle})" + ", ";
                }

                if (returnedPlaylistsInfo != "")
                    ToastMessage.InformationDialog("Done\n\nFollowing playlists were found available again:\n" + returnedPlaylistsInfo.TrimToLast(", "));
            }
        }

        /// <summary>
        /// Differentiates between deleted (unavailable) playlists, and ones that are private and belong to 
        /// a different accounnt, and then splits the playlists in these two groups returns them.
        /// </summary>
        public static (List<Playlist> deletedPlaylists, List<Playlist> privatePlaylists) SplitUnavailablePlaylist(List<string> nonExistentPlaylists)
        {
            List<Playlist> deletedPlaylists = new();
            List<Playlist> privatePlaylists = new();

            List<UserProfile> existingUserProfiles = OAuthSystem.ReadSavedUserProfiles();
            List<string> existingUsersChannelIds = existingUserProfiles.Select(user => user.ChannelId).ToList();

            foreach (var playlistId in nonExistentPlaylists)
            {
                var playlist = PlaylistsData.ReadPlaylistData(playlistId);
                var playlistChannelId = playlist.Snippet.ChannelId;

                // That's a very rare but possible case, don't remove that.
                if (playlist == null)
                    continue;

                if (existingUsersChannelIds.Contains(playlistChannelId))
                {
                    // If the playlist belongs to the current user and it wasn't retrieved - playlist has been deleted
                    if (GlobalItems.UserProfile.ChannelId == playlistChannelId)
                        deletedPlaylists.Add(playlist);
                    // Playlist is unavailable because it belongs to a different user on the app
                    // and is set to private (it has to be updated from that account)
                    else if (playlist.Status.PrivacyStatus == "private")
                    {
                        privatePlaylists.Add(playlist);
                    }
                    else
                        deletedPlaylists.Add(playlist);
                }
                else
                    // If it doesn't belong to any of the accounts in the app then the owner has deleted it or set it to private
                    deletedPlaylists.Add(playlist);
            }

            return (deletedPlaylists, privatePlaylists);
        }

    }
}
